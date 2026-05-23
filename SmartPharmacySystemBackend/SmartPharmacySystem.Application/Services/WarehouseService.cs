using AutoMapper;
using SmartPharmacySystem.Application.DTOs.Warehouses;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public WarehouseService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<WarehouseDto> GetByIdAsync(int id)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("المخزن غير موجود");
        return _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllAsync(int? branchId = null, WarehouseType? type = null)
    {
        var warehouses = await _unitOfWork.Warehouses.GetAllAsync(branchId, type);
        return _mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
    }

    public async Task<IEnumerable<WarehouseDto>> GetByBranchIdAsync(int branchId)
    {
        var warehouses = await _unitOfWork.Warehouses.GetByBranchIdAsync(branchId);
        return _mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
    }

    public async Task<WarehouseDto?> GetByBranchAndTypeAsync(int branchId, WarehouseType type)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByBranchAndTypeAsync(branchId, type);
        return warehouse != null ? _mapper.Map<WarehouseDto>(warehouse) : null;
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseDto dto)
    {
        var warehouse = _mapper.Map<Warehouse>(dto);
        await _unitOfWork.Warehouses.AddAsync(warehouse);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task UpdateAsync(UpdateWarehouseDto dto, int id)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("المخزن غير موجود");

        _mapper.Map(dto, warehouse);
        await _unitOfWork.Warehouses.UpdateAsync(warehouse);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("المخزن غير موجود");
        await _unitOfWork.Warehouses.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _unitOfWork.Warehouses.ExistsAsync(id);
    }

    public async Task<int> GetWarehouseCountAsync(int branchId)
    {
        return await _unitOfWork.Warehouses.GetWarehouseCountAsync(branchId);
    }
}
