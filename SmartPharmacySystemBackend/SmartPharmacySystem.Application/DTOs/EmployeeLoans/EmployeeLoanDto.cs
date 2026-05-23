namespace SmartPharmacySystem.Application.DTOs.EmployeeLoans;

/// <summary>
/// كائن نقل البيانات لعرض سلفة موظف.
/// </summary>
public class EmployeeLoanDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal MonthlyInstalment { get; set; }
    public decimal RemainingAmount { get; set; }
    public int StartMonth { get; set; }
    public int StartYear { get; set; }
    public bool IsFullyPaid { get; set; }
    public int InstalmentsPaid { get; set; }
    public int TotalInstalments { get; set; }
    public DateTime CreatedAt { get; set; }
}
