using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IEmployeeLoanRepository
{
    Task<EmployeeLoan?> GetByIdAsync(int id);
    Task<IEnumerable<EmployeeLoan>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<EmployeeLoan>> GetActiveLoansAsync(int? branchId = null);
    Task<IEnumerable<EmployeeLoan>> GetFullyPaidLoansAsync();
    Task<EmployeeLoan> AddAsync(EmployeeLoan loan);
    Task UpdateAsync(EmployeeLoan loan);
    Task DeleteAsync(int id);
    Task<decimal> GetTotalRemainingAsync(int employeeId);
}
