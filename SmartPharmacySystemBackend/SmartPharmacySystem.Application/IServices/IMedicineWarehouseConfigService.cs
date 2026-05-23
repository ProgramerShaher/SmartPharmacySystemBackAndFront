using SmartPharmacySystem.Application.DTOs.MedicineWarehouseConfigs;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IMedicineWarehouseConfigService
{
    Task<MedicineWarehouseConfigDto> GetByIdAsync(int warehouseId, int medicineId);
    Task<IEnumerable<MedicineWarehouseConfigDto>> GetByWarehouseIdAsync(int warehouseId);
    Task<IEnumerable<MedicineWarehouseConfigDto>> GetByMedicineIdAsync(int medicineId);
    Task<MedicineWarehouseConfigDto> CreateAsync(CreateMedicineWarehouseConfigDto dto);
    Task UpdateAsync(UpdateMedicineWarehouseConfigDto dto);
    Task DeleteAsync(int warehouseId, int medicineId);
    Task<bool> ExistsAsync(int warehouseId, int medicineId);
}
