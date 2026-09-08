using System.Threading.Tasks;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.IServices;

public interface IBackupStateService
{
    /// <summary>
    /// Changes the state of a backup and logs the transition.
    /// Also updates timestamps appropriately.
    /// </summary>
    Task TransitionStateAsync(int backupId, BackupStatus newStatus, string? errorMessage = null, string? verificationMessage = null);

    /// <summary>
    /// Runs at startup to find any backup that was running when the system crashed.
    /// Marks them as Failed so they don't remain stuck in intermediate states.
    /// </summary>
    Task MarkInterruptedBackupsAsFailedAsync();

    /// <summary>
    /// Checks if a backup with the given OccurrenceId exists and is already completed.
    /// </summary>
    Task<bool> IsOccurrenceAlreadyCompletedAsync(string occurrenceId);

    /// <summary>
    /// Updates NextScheduledRun only after a complete success or fatal failure of recovery.
    /// </summary>
    Task UpdateNextScheduledRunAsync(BackupConfiguration config, System.DateTime nextRunUtc);
}
