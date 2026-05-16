using SmartPharmacySystem.Application.DTOs.Settings;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IPharmacySettingsService
{
    Task<PharmacySettingsDto> GetSettingsAsync();
    Task<PharmacySettingsDto> UpdateSettingsAsync(UpdatePharmacySettingsDto dto);
}
