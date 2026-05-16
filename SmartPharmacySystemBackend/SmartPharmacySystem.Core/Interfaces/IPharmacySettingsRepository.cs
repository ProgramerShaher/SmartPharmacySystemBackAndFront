using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IPharmacySettingsRepository
{
    Task<PharmacySettings?> GetSettingsAsync();
    Task AddAsync(PharmacySettings settings);
    Task UpdateAsync(PharmacySettings settings);
}
