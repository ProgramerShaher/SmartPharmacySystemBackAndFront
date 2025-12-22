using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IAlertRepository
{
    Task<IEnumerable<Alert>> GetAllAsync();
    Task<Alert?> GetByIdAsync(int id);

    Task<IEnumerable<Alert>> GetByBatchIdAsync(int batchId);
    Task<IEnumerable<Alert>> GetByStatusAsync(AlertStatus status);
    Task<IEnumerable<Alert>> GetExpiringAlertsAsync(DateTime date);

    Task<bool> ExistsAsync(int id);

    Task AddAsync(Alert alert);
    Task UpdateAsync(Alert alert);
    Task DeleteAsync(int id);
}
