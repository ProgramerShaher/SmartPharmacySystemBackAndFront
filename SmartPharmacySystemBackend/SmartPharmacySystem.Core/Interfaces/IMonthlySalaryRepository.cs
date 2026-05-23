using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IMonthlySalaryRepository
{
    Task<MonthlySalary?> GetByIdAsync(int id);
    Task<MonthlySalary?> GetByEmployeeMonthYearAsync(int employeeId, int month, int year);
    Task<IEnumerable<MonthlySalary>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<MonthlySalary>> GetByBranchMonthYearAsync(int branchId, int month, int year);
    Task<IEnumerable<MonthlySalary>> GetByMonthYearAsync(int month, int year, int? branchId = null);
    Task<MonthlySalary> AddAsync(MonthlySalary salary);
    Task UpdateAsync(MonthlySalary salary);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int employeeId, int month, int year);
    Task<decimal> GetTotalPayrollAsync(int month, int year, int? branchId = null);
}
