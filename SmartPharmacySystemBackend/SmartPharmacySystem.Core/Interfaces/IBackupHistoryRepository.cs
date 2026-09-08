using System.Collections.Generic;
using System.Threading.Tasks;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IBackupHistoryRepository
{
    Task<BackupHistory?> GetByIdAsync(int id);
    Task<IEnumerable<BackupHistory>> GetAllAsync(int page, int pageSize);
    Task<int> GetTotalCountAsync();
    Task<IEnumerable<BackupHistory>> GetByStatusAsync(BackupStatus status);
    Task<BackupHistory?> GetByOccurrenceIdAsync(string occurrenceId);
    Task<IEnumerable<BackupHistory>> GetSuccessfulBackupsForRetentionAsync();
    Task AddAsync(BackupHistory history);
    Task UpdateAsync(BackupHistory history);
}
