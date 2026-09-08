using System.Threading.Tasks;

namespace SmartPharmacySystem.Application.IServices;

public interface IBackupRetentionService
{
    /// <summary>
    /// Applies the configured retention policy to delete old backups from Google Drive and the database.
    /// Ensures that the latest successful backup is NEVER deleted.
    /// </summary>
    Task ApplyRetentionPolicyAsync();

    /// <summary>
    /// Scans the local temporary directory and safely deletes orphaned or expired local files,
    /// while strictly preserving files belonging to running/pending operations or failed operations within the retry window.
    /// </summary>
    Task CleanupLocalFilesAsync();
}
