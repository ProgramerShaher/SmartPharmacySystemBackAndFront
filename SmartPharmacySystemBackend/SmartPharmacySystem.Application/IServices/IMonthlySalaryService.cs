using SmartPharmacySystem.Application.DTOs.MonthlySalaries;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IMonthlySalaryService
{
    Task<MonthlySalaryDto> GetByIdAsync(int id);
    Task<MonthlySalaryDto?> GetByEmployeeMonthYearAsync(int employeeId, int month, int year);
    Task<IEnumerable<MonthlySalaryDto>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<MonthlySalaryDto>> GetByBranchMonthYearAsync(int branchId, int month, int year);
    Task<IEnumerable<MonthlySalaryDto>> GetByMonthYearAsync(int month, int year, int? branchId = null);
    Task<MonthlySalaryDto> CreateAsync(CreateMonthlySalaryDto dto);
    Task UpdateAsync(UpdateMonthlySalaryDto dto);
    Task DeleteAsync(int id);
    Task<decimal> GetTotalPayrollAsync(int month, int year, int? branchId = null);
    Task<PayrollSummaryDto> GetPayrollSummaryAsync(int month, int year, int? branchId = null);
    Task<bool> ExistsAsync(int employeeId, int month, int year);
}
