using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Infrastructure.Services;

public class BackupStateService : IBackupStateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BackupStateService> _logger;

    public BackupStateService(IUnitOfWork unitOfWork, ILogger<BackupStateService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task TransitionStateAsync(int backupId, BackupStatus newStatus, string? errorMessage = null, string? verificationMessage = null)
    {
        var backup = await _unitOfWork.BackupHistories.GetByIdAsync(backupId);
        if (backup == null) throw new InvalidOperationException($"Backup {backupId} not found.");

        _logger.LogInformation($"Backup {backupId} transition: {backup.Status} -> {newStatus}");

        backup.Status = newStatus;
        backup.UpdatedAt = DateTime.UtcNow;

        if (errorMessage != null)
        {
            backup.ErrorMessage = errorMessage;
        }

        if (verificationMessage != null)
        {
            backup.VerificationMessage = verificationMessage;
            backup.IsVerified = true;
        }

        if (newStatus == BackupStatus.Completed || newStatus == BackupStatus.Failed)
        {
            backup.CompletedAt = DateTime.UtcNow;
            backup.DurationSeconds = (int)(backup.CompletedAt.Value - backup.StartedAt).TotalSeconds;
        }

        await _unitOfWork.BackupHistories.UpdateAsync(backup);
    }

    public async Task MarkInterruptedBackupsAsFailedAsync()
    {
        var activeStates = new[]
        {
            BackupStatus.Pending,
            BackupStatus.SqlBackupRunning,
            BackupStatus.VerificationRunning,
            BackupStatus.EncryptionRunning,
            BackupStatus.Uploading,
            BackupStatus.Retrying
        };

        foreach (var state in activeStates)
        {
            var interrupted = await _unitOfWork.BackupHistories.GetByStatusAsync(state);
            foreach (var backup in interrupted)
            {
                _logger.LogWarning($"Found interrupted backup {backup.Id} in state {state}. Marking as Failed.");
                await TransitionStateAsync(backup.Id, BackupStatus.Failed, "النظام توقف عن العمل بشكل مفاجئ أثناء تنفيذ هذه العملية.");
            }
        }
    }

    public async Task<bool> IsOccurrenceAlreadyCompletedAsync(string occurrenceId)
    {
        var backup = await _unitOfWork.BackupHistories.GetByOccurrenceIdAsync(occurrenceId);
        return backup != null;
    }

    public async Task UpdateNextScheduledRunAsync(BackupConfiguration config, DateTime nextRunUtc)
    {
        config.NextScheduledRun = nextRunUtc;
        config.LastRunAt = DateTime.UtcNow;
        config.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.BackupConfigurations.UpdateConfigurationAsync(config);
    }
}
