using SmartPharmacySystem.Application.DTOs.InventoryStocks;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IInventoryStockService
{
    Task<InventoryStockDto> GetByIdAsync(int id);
    Task<IEnumerable<InventoryStockDto>> GetByWarehouseIdAsync(int warehouseId);
    Task<IEnumerable<InventoryStockDto>> GetByMedicineIdAsync(int medicineId);
    Task<IEnumerable<InventoryStockDto>> GetExpiringSoonAsync(int daysThreshold);
    Task<IEnumerable<InventoryStockDto>> GetExpiredAsync();
    Task<IEnumerable<InventoryStockDto>> GetBelowReorderLevelAsync();
    Task<int> GetTotalQuantityAsync(int warehouseId, int medicineId);
    Task<InventoryStockDto> CreateAsync(CreateInventoryStockDto dto);
    Task UpdateAsync(UpdateInventoryStockDto dto);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<InventoryStockDto>> SearchAsync(string? search = null, int? warehouseId = null, int? medicineId = null);
}
