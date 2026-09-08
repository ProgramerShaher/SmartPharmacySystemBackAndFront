using System.Threading.Tasks;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.IServices;

public interface IBackupService
{
    /// <summary>
    /// Executes the complete backup lifecycle. This method is idempotent 
    /// and uses a SemaphoreSlim to prevent concurrent execution.
    /// </summary>
    Task ExecuteBackupAsync(BackupType type, string occurrenceId, int? triggeredByUserId = null, string? triggeredByUserName = null);
    
    /// <summary>
    /// Initiates a manual backup by creating a pending record and starting the execution.
    /// </summary>
    Task<BackupHistory> TriggerManualBackupAsync(int userId, string userName);
    
    /// <summary>
    /// Re-runs a failed backup.
    /// </summary>
    Task<BackupHistory> RetryBackupAsync(int backupId, int userId, string userName);
}
