using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByCodeAsync(string employeeCode);
    Task<IEnumerable<Employee>> GetAllAsync(int? branchId = null, int? departmentId = null, bool? isActive = null, string? search = null);
    Task<IEnumerable<Employee>> GetByBranchIdAsync(int branchId);
    Task<Employee> AddAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(int id);
    Task<bool> CodeExistsAsync(string employeeCode, int? excludeId = null);
    Task<bool> NationalIdExistsAsync(string nationalId, int? excludeId = null);
    Task<int> GetEmployeeCountAsync(int branchId);
    Task<IEnumerable<Employee>> GetActiveEmployeesAsync();
}
