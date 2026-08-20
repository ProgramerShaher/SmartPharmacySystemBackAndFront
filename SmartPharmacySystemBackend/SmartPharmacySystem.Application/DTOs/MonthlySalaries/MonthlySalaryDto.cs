using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.MonthlySalaries;

public class MonthlySalaryDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalBonuses { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentStatusName { get; set; } = string.Empty;
    public string PaymentStatusColor { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public int? PaidFromAccountId { get; set; }
    public string PaidFromAccountName { get; set; } = string.Empty;
    public int? SalaryExpenseAccountId { get; set; }
    public string SalaryExpenseAccountName { get; set; } = string.Empty;
    public int? JournalEntryId { get; set; }
    public List<SalaryDeductionItemDto> Deductions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class PaySalaryDto
{
    public int PaidFromAccountId { get; set; }
    public int? SalaryExpenseAccountId { get; set; }
}

public class CreateMonthlySalaryDto
{
    public int EmployeeId { get; set; }
    public int BranchId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalBonuses { get; set; }
    public decimal TotalDeductions { get; set; }
    public List<CreateSalaryDeductionItemDto>? Deductions { get; set; }
}

public class UpdateMonthlySalaryDto
{
    public int Id { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalBonuses { get; set; }
    public decimal TotalDeductions { get; set; }
    public List<CreateSalaryDeductionItemDto>? Deductions { get; set; }
}

public class MonthlySalaryQueryDto
{
    public int? BranchId { get; set; }
    public int? EmployeeId { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
}

public class PayrollSummaryDto
{
    public int TotalEmployees { get; set; }
    public int PaidCount { get; set; }
    public int PendingCount { get; set; }
    public decimal TotalBasicSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalBonuses { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetSalary { get; set; }
    public int? PaidFromAccountId { get; set; }
    public string PaidFromAccountName { get; set; } = string.Empty;
}

public class CreateSalaryDeductionItemDto
{
    public string DeductionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
