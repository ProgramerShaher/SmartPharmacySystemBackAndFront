// SmartPharmacySystem.Application/Interfaces/IAlertService.cs
namespace SmartPharmacySystem.Application.Interfaces;

using SmartPharmacySystem.Application.DTOs.Alerts;
using SmartPharmacySystem.Core.Enums;

public interface IAlertService
{
    Task<IEnumerable<AlertDto>> GetAllAsync();
    Task<AlertDto> GetByIdAsync(int id);

    Task<AlertDto> CreateAsync(CreateAlertDto dto);
    Task<AlertDto> UpdateAsync(int id, UpdateAlertDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<AlertDto>> GetByBatchIdAsync(int batchId);
    Task<IEnumerable<AlertDto>> GetByStatusAsync(AlertStatus status);
    Task MarkAsReadAsync(int id);
    Task GenerateExpiryAlertsAsync();
    Task GenerateLowStockAlertsAsync();
    Task SyncMedicineAlertsAsync(int medicineId);
    Task<IEnumerable<SmartPharmacySystem.Application.DTOs.Notifications.ExpiryAlertDto>> GetRealTimeExpiryAlertsAsync();

    // Unified Alert System
    Task<IEnumerable<UnifiedAlertDto>> GetActiveSystemAlertsAsync();
}