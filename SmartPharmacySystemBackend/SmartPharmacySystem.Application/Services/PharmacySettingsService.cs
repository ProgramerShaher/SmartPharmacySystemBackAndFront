using AutoMapper;
using SmartPharmacySystem.Application.DTOs.Settings;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class PharmacySettingsService : IPharmacySettingsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PharmacySettingsService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PharmacySettingsDto> GetSettingsAsync()
    {
        var settings = await _unitOfWork.PharmacySettings.GetSettingsAsync();
        
        // If no settings exist yet, create default settings
        if (settings == null)
        {
            settings = new PharmacySettings 
            {
                PharmacyName = "صيدلية جديدة",
                BaseCurrency = "ر.س"
            };
            await _unitOfWork.PharmacySettings.AddAsync(settings);
            await _unitOfWork.SaveChangesAsync();
        }

        return _mapper.Map<PharmacySettingsDto>(settings);
    }

    public async Task<PharmacySettingsDto> UpdateSettingsAsync(UpdatePharmacySettingsDto dto)
    {
        var settings = await _unitOfWork.PharmacySettings.GetSettingsAsync();

        if (settings == null)
        {
            settings = new PharmacySettings();
            _mapper.Map(dto, settings);
            await _unitOfWork.PharmacySettings.AddAsync(settings);
        }
        else
        {
            _mapper.Map(dto, settings);
            await _unitOfWork.PharmacySettings.UpdateAsync(settings);
        }

        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<PharmacySettingsDto>(settings);
    }
}
