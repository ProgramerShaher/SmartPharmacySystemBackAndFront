using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces
{
    public interface IPriceOverrideRepository
    {
        Task AddAsync(PriceOverride entity);
        Task<IEnumerable<PriceOverride>> GetAllAsync();
        Task<IEnumerable<PriceOverride>> GetByMedicineIdAsync(int medicineId);
    }
}
