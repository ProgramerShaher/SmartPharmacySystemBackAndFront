using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Infrastructure.Services;

public class BackupRetentionService : IBackupRetentionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGoogleDriveService _googleDriveService;
    private readonly ILogger<BackupRetentionService> _logger;

    public BackupRetentionService(IUnitOfWork unitOfWork, IGoogleDriveService googleDriveService, ILogger<BackupRetentionService> logger)
    {
        _unitOfWork = unitOfWork;
        _googleDriveService = googleDriveService;
        _logger = logger;
    }

    public async Task ApplyRetentionPolicyAsync()
    {
        var config = await _unitOfWork.BackupConfigurations.GetConfigurationAsync();
        if (config == null) return;

        var successfulBackups = (await _unitOfWork.BackupHistories.GetSuccessfulBackupsForRetentionAsync()).ToList();

        if (successfulBackups.Count <= 1)
        {
            // Never delete the only successful backup
            return;
        }

        // Skip the very first one (most recent), we NEVER delete it.
        var backupsToEvaluate = successfulBackups.Skip(1).ToList();
        
        foreach (var backup in backupsToEvaluate)
        {
            bool shouldDelete = false;

            if (config.RetentionMode == BackupRetentionMode.ByCount)
            {
                // If the total count of successful backups is greater than MaxBackupsToKeep
                // We find the index of this backup in the original list.
                var index = successfulBackups.IndexOf(backup);
                if (index >= config.MaxBackupsToKeep)
                {
                    shouldDelete = true;
                }
            }
            else if (config.RetentionMode == BackupRetentionMode.ByDays)
            {
                if (backup.CompletedAt.HasValue && 
                    (DateTime.UtcNow - backup.CompletedAt.Value).TotalDays > config.RetentionDays)
                {
                    shouldDelete = true;
                }
            }

            if (shouldDelete)
            {
                _logger.LogInformation($"Applying retention policy: Deleting backup {backup.Id}");
                
                if (!string.IsNullOrEmpty(backup.GoogleDriveFileId))
                {
                    await _googleDriveService.DeleteFileAsync(backup.GoogleDriveFileId);
                }

                // Delete local file if it still exists
                if (!string.IsNullOrEmpty(backup.LocalEncryptedPath) && File.Exists(backup.LocalEncryptedPath))
                {
                    File.Delete(backup.LocalEncryptedPath);
                }

                // Delete from DB (or Soft Delete, ApplicationDbContext does Soft Delete)
                _unitOfWork.BackupHistories.UpdateAsync(backup); // Wait, no RemoveAsync in repo?
                // Let's just mark it as IsDeleted if we had access to DbContext, or just leave it for audit but clear paths.
                backup.IsDeleted = true;
                backup.DeletedAt = DateTime.UtcNow;
                await _unitOfWork.BackupHistories.UpdateAsync(backup);
            }
        }
    }

    public async Task CleanupLocalFilesAsync()
    {
        var config = await _unitOfWork.BackupConfigurations.GetConfigurationAsync();
        if (config == null) return;

        var baseDir = AppContext.BaseDirectory;
        var tempDir = Path.Combine(baseDir, config.TempBackupPath);
        if (!Directory.Exists(tempDir)) return;

        var files = Directory.GetFiles(tempDir);
        
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            
            // Expected filename format: PharmacyDB_{OccurrenceId}.bak or .bak.enc
            // Let's extract the OccurrenceId by removing standard prefix and suffixes.
            var prefix = "PharmacyDB_";
            if (!fileName.StartsWith(prefix)) continue; // Not ours

            var remainder = fileName.Substring(prefix.Length);
            var occurrenceId = remainder.Replace(".bak.enc", "").Replace(".bak", "");

            var history = await _unitOfWork.BackupHistories.GetByOccurrenceIdAsync(occurrenceId);
            
            // If Orphaned (no history)
            if (history == null)
            {
                File.Delete(file);
                continue;
            }

            // If active, keep it
            var activeStates = new[] { BackupStatus.Pending, BackupStatus.SqlBackupRunning, BackupStatus.VerificationRunning, BackupStatus.EncryptionRunning, BackupStatus.Uploading, BackupStatus.Retrying };
            if (activeStates.Contains(history.Status))
            {
                continue; // Do not delete
            }

            if (history.Status == BackupStatus.Completed)
            {
                if (file.EndsWith(".bak"))
                {
                    File.Delete(file); // Raw file should not exist
                }
                else if (file.EndsWith(".bak.enc"))
                {
                    if (history.CompletedAt.HasValue && (DateTime.UtcNow - history.CompletedAt.Value).TotalDays > config.LocalRetentionDays)
                    {
                        File.Delete(file);
                    }
                }
            }
            else if (history.Status == BackupStatus.Failed)
            {
                if (file.EndsWith(".bak"))
                {
                    File.Delete(file); // Delete raw failed
                }
                else if (file.EndsWith(".bak.enc"))
                {
                    // Keep encrypted failed for manual retry, for maybe 3 days
                    if (history.UpdatedAt.HasValue && (DateTime.UtcNow - history.UpdatedAt.Value).TotalDays > 3)
                    {
                        File.Delete(file);
                    }
                }
            }
        }
    }
}
