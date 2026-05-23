using SmartPharmacySystem.Application.DTOs.Employees;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IEmployeeService
{
    Task<EmployeeDto> GetByIdAsync(int id);
    Task<EmployeeDto> GetByCodeAsync(string employeeCode);
    Task<IEnumerable<EmployeeDto>> GetAllAsync(int? branchId = null, int? departmentId = null, bool? isActive = null, string? search = null);
    Task<IEnumerable<EmployeeDto>> GetByBranchIdAsync(int branchId);
    Task<IEnumerable<EmployeeDto>> GetActiveEmployeesAsync();
    Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto);
    Task UpdateAsync(UpdateEmployeeDto dto);
    Task DeleteAsync(int id);
    Task<bool> CodeExistsAsync(string employeeCode, int? excludeId = null);
    Task<bool> NationalIdExistsAsync(string nationalId, int? excludeId = null);
    Task<int> GetEmployeeCountAsync(int branchId);
}
