using AutoMapper;
using SmartPharmacySystem.Application.DTOs.PriceOverrides;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class PriceOverrideService : IPriceOverrideService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PriceOverrideService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PriceOverrideDto>> GetAllAsync()
    {
        var overrides = await _unitOfWork.PriceOverrides.GetAllAsync();
        return _mapper.Map<IEnumerable<PriceOverrideDto>>(overrides);
    }

    public async Task<IEnumerable<PriceOverrideDto>> GetByMedicineIdAsync(int medicineId)
    {
        var overrides = await _unitOfWork.PriceOverrides.GetByMedicineIdAsync(medicineId);
        return _mapper.Map<IEnumerable<PriceOverrideDto>>(overrides);
    }
}
