using AutoMapper;
using SmartPharmacySystem.Application.DTOs.Employees;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EmployeeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<EmployeeDto> GetByIdAsync(int id)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("الموظف غير موجود");
        return _mapper.Map<EmployeeDto>(employee);
    }

    public async Task<EmployeeDto> GetByCodeAsync(string employeeCode)
    {
        var employee = await _unitOfWork.Employees.GetByCodeAsync(employeeCode)
            ?? throw new KeyNotFoundException("الموظف غير موجود");
        return _mapper.Map<EmployeeDto>(employee);
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(int? branchId = null, int? departmentId = null, bool? isActive = null, string? search = null)
    {
        var employees = await _unitOfWork.Employees.GetAllAsync(branchId, departmentId, isActive, search);
        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<IEnumerable<EmployeeDto>> GetByBranchIdAsync(int branchId)
    {
        var employees = await _unitOfWork.Employees.GetByBranchIdAsync(branchId);
        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<IEnumerable<EmployeeDto>> GetActiveEmployeesAsync()
    {
        var employees = await _unitOfWork.Employees.GetActiveEmployeesAsync();
        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
    {
        if (await _unitOfWork.Employees.CodeExistsAsync(dto.EmployeeCode))
            throw new InvalidOperationException("كود الموظف موجود بالفعل");

        if (!string.IsNullOrEmpty(dto.NationalId) && await _unitOfWork.Employees.NationalIdExistsAsync(dto.NationalId))
            throw new InvalidOperationException("رقم الهوية موجود بالفعل");

        var employee = _mapper.Map<Employee>(dto);
        await _unitOfWork.Employees.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<EmployeeDto>(employee);
    }

    public async Task UpdateAsync(UpdateEmployeeDto dto)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException("الموظف غير موجود");

        if (await _unitOfWork.Employees.CodeExistsAsync(dto.EmployeeCode, dto.Id))
            throw new InvalidOperationException("كود الموظف موجود بالفعل");

        if (!string.IsNullOrEmpty(dto.NationalId) && await _unitOfWork.Employees.NationalIdExistsAsync(dto.NationalId, dto.Id))
            throw new InvalidOperationException("رقم الهوية موجود بالفعل");

        _mapper.Map(dto, employee);
        await _unitOfWork.Employees.UpdateAsync(employee);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("الموظف غير موجود");
        await _unitOfWork.Employees.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> CodeExistsAsync(string employeeCode, int? excludeId = null)
    {
        return await _unitOfWork.Employees.CodeExistsAsync(employeeCode, excludeId);
    }

    public async Task<bool> NationalIdExistsAsync(string nationalId, int? excludeId = null)
    {
        return await _unitOfWork.Employees.NationalIdExistsAsync(nationalId, excludeId);
    }

    public async Task<int> GetEmployeeCountAsync(int branchId)
    {
        return await _unitOfWork.Employees.GetEmployeeCountAsync(branchId);
    }

    public async Task<BranchEmployeesDashboardDto> GetBranchDashboardAsync(int branchId)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(branchId) 
                     ?? throw new KeyNotFoundException("الفرع غير موجود");
                     
        var employees = await _unitOfWork.Employees.GetByBranchIdAsync(branchId);
        
        var dashboard = new BranchEmployeesDashboardDto
        {
            BranchId = branchId,
            BranchName = branch.Name,
            TotalEmployees = employees.Count(),
            TotalBranchSalaries = employees.Sum(e => e.BasicSalary),
            Departments = new List<DepartmentEmployeesDto>()
        };

        var groupedByDept = employees.GroupBy(e => new { e.DepartmentId, e.Department?.Name });
        
        foreach (var group in groupedByDept)
        {
            var deptDto = new DepartmentEmployeesDto
            {
                DepartmentId = group.Key.DepartmentId,
                DepartmentName = group.Key.Name ?? $"قسم رقم {group.Key.DepartmentId}",
                EmployeeCount = group.Count(),
                TotalBasicSalary = group.Sum(e => e.BasicSalary),
                Employees = _mapper.Map<List<EmployeeDto>>(group.ToList())
            };
            dashboard.Departments.Add(deptDto);
        }

        return dashboard;
    }
}
