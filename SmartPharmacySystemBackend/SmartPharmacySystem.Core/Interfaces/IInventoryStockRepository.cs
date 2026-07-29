using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IInventoryStockRepository
{
    Task<InventoryStock?> GetByIdAsync(int id);
    Task<IEnumerable<InventoryStock>> GetByWarehouseIdAsync(int warehouseId);
    Task<IEnumerable<InventoryStock>> GetByMedicineIdAsync(int medicineId);
    Task<IEnumerable<InventoryStock>> GetByWarehouseAndMedicineAsync(int warehouseId, int medicineId);
    Task<InventoryStock?> GetByWarehouseMedicineBatchAsync(int warehouseId, int medicineId, string batchNumber);
    Task<IEnumerable<InventoryStock>> GetExpiringSoonAsync(int daysThreshold);
    Task<IEnumerable<InventoryStock>> GetExpiredAsync();
    Task<IEnumerable<InventoryStock>> GetBelowReorderLevelAsync();
    Task<int> GetTotalQuantityAsync(int warehouseId, int medicineId);
    Task<InventoryStock> AddAsync(InventoryStock stock);
    Task UpdateAsync(InventoryStock stock);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<InventoryStock>> SearchAsync(string? search = null, int? warehouseId = null, int? medicineId = null);
    Task<IEnumerable<InventoryStock>> GetStocksForBatchesAsync(IEnumerable<int> medicineIds, IEnumerable<string> batchNumbers);
}
