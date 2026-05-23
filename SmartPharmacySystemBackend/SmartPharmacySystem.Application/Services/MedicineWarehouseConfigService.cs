using AutoMapper;
using SmartPharmacySystem.Application.DTOs.MedicineWarehouseConfigs;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class MedicineWarehouseConfigService : IMedicineWarehouseConfigService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MedicineWarehouseConfigService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MedicineWarehouseConfigDto> GetByIdAsync(int warehouseId, int medicineId)
    {
        var config = await _unitOfWork.MedicineWarehouseConfigs.GetByIdAsync(warehouseId, medicineId)
            ?? throw new KeyNotFoundException("إعدادات الدواء في المخزن غير موجودة");
        return _mapper.Map<MedicineWarehouseConfigDto>(config);
    }

    public async Task<IEnumerable<MedicineWarehouseConfigDto>> GetByWarehouseIdAsync(int warehouseId)
    {
        var configs = await _unitOfWork.MedicineWarehouseConfigs.GetByWarehouseIdAsync(warehouseId);
        return _mapper.Map<IEnumerable<MedicineWarehouseConfigDto>>(configs);
    }

    public async Task<IEnumerable<MedicineWarehouseConfigDto>> GetByMedicineIdAsync(int medicineId)
    {
        var configs = await _unitOfWork.MedicineWarehouseConfigs.GetByMedicineIdAsync(medicineId);
        return _mapper.Map<IEnumerable<MedicineWarehouseConfigDto>>(configs);
    }

    public async Task<MedicineWarehouseConfigDto> CreateAsync(CreateMedicineWarehouseConfigDto dto)
    {
        if (await _unitOfWork.MedicineWarehouseConfigs.ExistsAsync(dto.WarehouseId, dto.MedicineId))
            throw new InvalidOperationException("إعدادات الدواء موجودة بالفعل في هذا المخزن");

        var config = _mapper.Map<MedicineWarehouseConfig>(dto);
        await _unitOfWork.MedicineWarehouseConfigs.AddAsync(config);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<MedicineWarehouseConfigDto>(config);
    }

    public async Task UpdateAsync(UpdateMedicineWarehouseConfigDto dto)
    {
        var config = await _unitOfWork.MedicineWarehouseConfigs.GetByIdAsync(dto.WarehouseId, dto.MedicineId)
            ?? throw new KeyNotFoundException("إعدادات الدواء في المخزن غير موجودة");

        _mapper.Map(dto, config);
        await _unitOfWork.MedicineWarehouseConfigs.UpdateAsync(config);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int warehouseId, int medicineId)
    {
        var config = await _unitOfWork.MedicineWarehouseConfigs.GetByIdAsync(warehouseId, medicineId)
            ?? throw new KeyNotFoundException("إعدادات الدواء في المخزن غير موجودة");
        await _unitOfWork.MedicineWarehouseConfigs.DeleteAsync(warehouseId, medicineId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int warehouseId, int medicineId)
    {
        return await _unitOfWork.MedicineWarehouseConfigs.ExistsAsync(warehouseId, medicineId);
    }
}
