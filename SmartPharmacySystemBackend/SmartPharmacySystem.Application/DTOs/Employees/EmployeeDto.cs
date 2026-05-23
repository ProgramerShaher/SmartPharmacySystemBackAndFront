namespace SmartPharmacySystem.Application.DTOs.Employees;

/// <summary>
/// كائن نقل البيانات لعرض معلومات موظف.
/// </summary>
public class EmployeeDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public decimal BasicSalary { get; set; }
    public bool IsActive { get; set; }
    public decimal TotalLoans { get; set; }
    public decimal RemainingLoans { get; set; }
    public DateTime CreatedAt { get; set; }
}
