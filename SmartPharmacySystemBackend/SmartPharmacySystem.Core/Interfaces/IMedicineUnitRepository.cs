using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// Repository interface for MedicineUnit (packaging/sale units per medicine).
/// </summary>
public interface IMedicineUnitRepository
{
    Task<MedicineUnit?> GetByIdAsync(int id);
    Task<IEnumerable<MedicineUnit>> GetByMedicineIdAsync(int medicineId);
    Task<IEnumerable<MedicineUnit>> GetAllowedForSaleAsync(int medicineId);
    Task<IEnumerable<MedicineUnit>> GetAllowedForPurchaseAsync(int medicineId);
    Task AddAsync(MedicineUnit entity);
    Task UpdateAsync(MedicineUnit entity);
    Task DeleteAsync(int id);
}
