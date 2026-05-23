using SmartPharmacySystem.Application.DTOs.EmployeeLoans;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IEmployeeLoanService
{
    Task<EmployeeLoanDto> GetByIdAsync(int id);
    Task<IEnumerable<EmployeeLoanDto>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<EmployeeLoanDto>> GetActiveLoansAsync(int? branchId = null);
    Task<IEnumerable<EmployeeLoanDto>> GetFullyPaidLoansAsync();
    Task<EmployeeLoanDto> CreateAsync(CreateEmployeeLoanDto dto);
    Task UpdateAsync(UpdateEmployeeLoanDto dto);
    Task DeleteAsync(int id);
    Task<EmployeeLoanDto> RecordPaymentAsync(int loanId, decimal amount);
    Task<decimal> GetTotalRemainingAsync(int employeeId);
}
