using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IMedicineWarehouseConfigRepository
{
    Task<MedicineWarehouseConfig?> GetByIdAsync(int warehouseId, int medicineId);
    Task<IEnumerable<MedicineWarehouseConfig>> GetByWarehouseIdAsync(int warehouseId);
    Task<IEnumerable<MedicineWarehouseConfig>> GetByMedicineIdAsync(int medicineId);
    Task<MedicineWarehouseConfig?> GetByWarehouseMedicineAsync(int warehouseId, int medicineId);
    Task<MedicineWarehouseConfig> AddAsync(MedicineWarehouseConfig config);
    Task UpdateAsync(MedicineWarehouseConfig config);
    Task DeleteAsync(int warehouseId, int medicineId);
    Task<bool> ExistsAsync(int warehouseId, int medicineId);
}
