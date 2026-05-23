using SmartPharmacySystem.Application.DTOs.PriceOverrides;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IPriceOverrideService
{
    Task<IEnumerable<PriceOverrideDto>> GetAllAsync();
    Task<IEnumerable<PriceOverrideDto>> GetByMedicineIdAsync(int medicineId);
}
