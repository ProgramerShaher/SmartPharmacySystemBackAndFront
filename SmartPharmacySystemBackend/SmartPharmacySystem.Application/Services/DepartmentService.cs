using AutoMapper;
using SmartPharmacySystem.Application.DTOs.Departments;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DepartmentDto> GetByIdAsync(int id)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("القسم غير موجود");
        return _mapper.Map<DepartmentDto>(department);
    }

    public async Task<DepartmentDto> GetByNameAsync(string name)
    {
        var department = await _unitOfWork.Departments.GetByNameAsync(name)
            ?? throw new KeyNotFoundException("القسم غير موجود");
        return _mapper.Map<DepartmentDto>(department);
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllAsync(string? search = null)
    {
        var departments = await _unitOfWork.Departments.GetAllAsync(search);
        return _mapper.Map<IEnumerable<DepartmentDto>>(departments);
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        if (await _unitOfWork.Departments.NameExistsAsync(dto.Name))
            throw new InvalidOperationException("اسم القسم موجود بالفعل");

        var department = _mapper.Map<Department>(dto);
        await _unitOfWork.Departments.AddAsync(department);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<DepartmentDto>(department);
    }

    public async Task UpdateAsync(UpdateDepartmentDto dto)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException("القسم غير موجود");

        if (await _unitOfWork.Departments.NameExistsAsync(dto.Name, dto.Id))
            throw new InvalidOperationException("اسم القسم موجود بالفعل");

        _mapper.Map(dto, department);
        await _unitOfWork.Departments.UpdateAsync(department);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("القسم غير موجود");
        await _unitOfWork.Departments.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        return await _unitOfWork.Departments.NameExistsAsync(name, excludeId);
    }
}
