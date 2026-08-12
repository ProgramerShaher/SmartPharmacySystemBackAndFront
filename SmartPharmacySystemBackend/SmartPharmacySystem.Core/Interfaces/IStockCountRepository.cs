using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IStockCountRepository
{
    Task<StockCountHeader?> GetHeaderByIdAsync(int id);
    Task<StockCountHeader?> GetByCodeAsync(string countCode);
    Task<IEnumerable<StockCountHeader>> GetAllHeadersAsync(int? warehouseId = null, StockCountStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<IEnumerable<StockCountHeader>> GetPendingApprovalAsync();
    Task<StockCountHeader> AddHeaderAsync(StockCountHeader header);
    Task UpdateHeaderAsync(StockCountHeader header);
    Task DeleteHeaderAsync(int id);
    Task<bool> CodeExistsAsync(string countCode, int? excludeId = null);

    Task<StockCountItem?> GetItemByIdAsync(int headerId, int medicineId, string batchNumber);
    Task<IEnumerable<StockCountItem>> GetItemsByHeaderIdAsync(int headerId);
    Task<StockCountItem> AddItemAsync(StockCountItem item);
    Task UpdateItemAsync(StockCountItem item);
    Task DeleteItemAsync(int headerId, int medicineId, string batchNumber);

    // Schedules
    Task<StockCountSchedule?> GetScheduleByIdAsync(int id);
    Task<IEnumerable<StockCountSchedule>> GetAllSchedulesAsync(int? warehouseId = null);
    Task<IEnumerable<StockCountSchedule>> GetDueSchedulesAsync();
    Task<StockCountSchedule> AddScheduleAsync(StockCountSchedule schedule);
    Task UpdateScheduleAsync(StockCountSchedule schedule);
    Task DeleteScheduleAsync(int id);
}
