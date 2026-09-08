using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Infrastructure.Services;

public class BackupService : IBackupService
{
    private static readonly SemaphoreSlim _executionLock = new(1, 1);
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackupService> _logger;
    private readonly IConfiguration _configuration;

    public BackupService(IServiceProvider serviceProvider, ILogger<BackupService> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<BackupHistory> TriggerManualBackupAsync(int userId, string userName)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var config = await unitOfWork.BackupConfigurations.GetConfigurationAsync();

        if (config == null) throw new InvalidOperationException("Backup configuration not found.");

        var occurrenceId = $"Manual_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

        var history = new BackupHistory
        {
            OccurrenceId = occurrenceId,
            BackupType = BackupType.Manual,
            Status = BackupStatus.Pending,
            StartedAt = DateTime.UtcNow,
            TriggeredByUserId = userId,
            TriggeredByUserName = userName
        };

        await unitOfWork.BackupHistories.AddAsync(history);

        // Run in background so API can return quickly
        _ = Task.Run(() => ExecuteBackupInternalAsync(history.Id, config));

        return history;
    }

    public async Task<BackupHistory> RetryBackupAsync(int backupId, int userId, string userName)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        
        var original = await unitOfWork.BackupHistories.GetByIdAsync(backupId);
        if (original == null) throw new InvalidOperationException("Original backup not found.");
        
        if (original.Status == BackupStatus.Completed)
            throw new InvalidOperationException("Backup is already completed.");

        var config = await unitOfWork.BackupConfigurations.GetConfigurationAsync();
        if (config == null) throw new InvalidOperationException("Backup configuration not found.");

        var retry = new BackupHistory
        {
            OccurrenceId = original.OccurrenceId,
            BackupType = original.BackupType,
            Status = BackupStatus.Pending,
            StartedAt = DateTime.UtcNow,
            TriggeredByUserId = userId,
            TriggeredByUserName = userName,
            IsRecovery = original.IsRecovery,
            OriginalBackupId = backupId,
            RetryCount = original.RetryCount + 1
        };

        await unitOfWork.BackupHistories.AddAsync(retry);

        // Run in background
        _ = Task.Run(() => ExecuteBackupInternalAsync(retry.Id, config));

        return retry;
    }

    public async Task ExecuteBackupAsync(BackupType type, string occurrenceId, int? triggeredByUserId = null, string? triggeredByUserName = null)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        
        var config = await unitOfWork.BackupConfigurations.GetConfigurationAsync();
        if (config == null) return;

        var history = new BackupHistory
        {
            OccurrenceId = occurrenceId,
            BackupType = type,
            Status = BackupStatus.Pending,
            StartedAt = DateTime.UtcNow,
            IsRecovery = type == BackupType.Automatic, // Will be set properly by scheduler
            TriggeredByUserId = triggeredByUserId,
            TriggeredByUserName = triggeredByUserName
        };

        await unitOfWork.BackupHistories.AddAsync(history);
        
        await ExecuteBackupInternalAsync(history.Id, config);
    }

    private async Task ExecuteBackupInternalAsync(int backupId, BackupConfiguration config)
    {
        // 1. Acquire execution lock
        if (!await _executionLock.WaitAsync(TimeSpan.Zero))
        {
            _logger.LogWarning("Backup already in progress. Skipping execution.");
            using var scope = _serviceProvider.CreateScope();
            var backupStateService = scope.ServiceProvider.GetRequiredService<IBackupStateService>();
            await backupStateService.TransitionStateAsync(backupId, BackupStatus.Failed, "يوجد نسخ احتياطي قيد التنفيذ حالياً. تم الإلغاء لتفادي التكرار.");
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var backupStateService = scope.ServiceProvider.GetRequiredService<IBackupStateService>();
            var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
            var keyManagementService = scope.ServiceProvider.GetRequiredService<IKeyManagementService>();
            var googleDriveService = scope.ServiceProvider.GetRequiredService<IGoogleDriveService>();
            
            var history = await unitOfWork.BackupHistories.GetByIdAsync(backupId);
            if (history == null) return;
            
            // Recheck if occurrence is already completed on Drive (Idempotency)
            if (history.IsRecovery)
            {
                var isCompleted = await backupStateService.IsOccurrenceAlreadyCompletedAsync(history.OccurrenceId);
                if (isCompleted)
                {
                    await backupStateService.TransitionStateAsync(backupId, BackupStatus.Completed, "تم تأكيد الرفع مسبقاً.");
                    return;
                }
            }

            var baseDir = AppContext.BaseDirectory;
            var tempDir = Path.Combine(baseDir, config.TempBackupPath);
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            var dbName = history.DatabaseName;
            var rawFileName = $"{dbName}_{history.OccurrenceId}.bak";
            var encFileName = $"{rawFileName}.enc";
            var rawFilePath = Path.Combine(tempDir, rawFileName);
            var encFilePath = Path.Combine(tempDir, encFileName);

            history.LocalTempPath = rawFilePath;
            await unitOfWork.BackupHistories.UpdateAsync(history);

            // 2. SQL Backup
            await backupStateService.TransitionStateAsync(backupId, BackupStatus.SqlBackupRunning);
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var sql = $"BACKUP DATABASE [{dbName}] TO DISK = @path WITH COMPRESSION, CHECKSUM, INIT, STATS = 10;";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 3600; // 1 hour
                    cmd.Parameters.AddWithValue("@path", rawFilePath);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            history.FileName = encFileName; // Ultimately we store the enc file name
            var rawFileInfo = new FileInfo(rawFilePath);
            
            // 3. Verification
            if (config.VerifyAfterBackup)
            {
                await backupStateService.TransitionStateAsync(backupId, BackupStatus.VerificationRunning);
                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    var sql = $"RESTORE VERIFYONLY FROM DISK = @path WITH CHECKSUM;";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandTimeout = 3600;
                        cmd.Parameters.AddWithValue("@path", rawFilePath);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }

            // 4. Encryption
            if (config.EncryptBackup)
            {
                await backupStateService.TransitionStateAsync(backupId, BackupStatus.EncryptionRunning);
                var key = keyManagementService.LoadKey();
                if (key == null)
                {
                    throw new InvalidOperationException("Encryption key not found. Please initialize the key management system.");
                }

                await encryptionService.EncryptFileAsync(rawFilePath, encFilePath, key, history.OccurrenceId);
                
                // Cleanup raw file immediately after successful encryption to save space
                if (File.Exists(rawFilePath)) File.Delete(rawFilePath);
                
                history.LocalTempPath = null;
                history.LocalEncryptedPath = encFilePath;
                history.FileSizeBytes = new FileInfo(encFilePath).Length;
                await unitOfWork.BackupHistories.UpdateAsync(history);
            }
            else
            {
                // If not encrypting, just use raw file (fallback, but requirement says we must encrypt)
                history.LocalEncryptedPath = rawFilePath;
                history.FileSizeBytes = rawFileInfo.Length;
                await unitOfWork.BackupHistories.UpdateAsync(history);
            }

            // 5. Google Drive Upload
            if (!string.IsNullOrEmpty(config.GoogleDriveFolderId))
            {
                await backupStateService.TransitionStateAsync(backupId, BackupStatus.Uploading);
                var pathToUpload = history.LocalEncryptedPath;
                var fileNameToUpload = history.FileName;

                var fileId = await googleDriveService.UploadFileAsync(pathToUpload, fileNameToUpload, config.GoogleDriveFolderId);
                history.GoogleDriveFileId = fileId;
                
                // Verify Upload
                var isVerified = await googleDriveService.VerifyUploadAsync(fileId, history.FileSizeBytes.Value, config.GoogleDriveFolderId);
                if (!isVerified)
                {
                    throw new Exception("Google Drive verification failed. File size or location mismatch.");
                }

                history.UploadStatus = UploadStatus.Completed;
            }
            else
            {
                history.UploadStatus = UploadStatus.Skipped;
            }

            // 6. Complete
            await backupStateService.TransitionStateAsync(backupId, BackupStatus.Completed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Backup {backupId} failed.");
            using var scope = _serviceProvider.CreateScope();
            var backupStateService = scope.ServiceProvider.GetRequiredService<IBackupStateService>();
            await backupStateService.TransitionStateAsync(backupId, BackupStatus.Failed, ex.Message);
        }
        finally
        {
            _executionLock.Release();
        }
    }
}
