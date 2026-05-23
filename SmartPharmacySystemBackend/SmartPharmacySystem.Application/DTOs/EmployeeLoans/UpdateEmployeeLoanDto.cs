namespace SmartPharmacySystem.Application.DTOs.EmployeeLoans;

public class UpdateEmployeeLoanDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public decimal MonthlyInstalment { get; set; }
    public int StartMonth { get; set; }
    public int StartYear { get; set; }
}
