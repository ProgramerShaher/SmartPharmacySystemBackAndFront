namespace SmartPharmacySystem.Application.DTOs.MonthlySalaries;

/// <summary>
/// كائن نقل البيانات لعرض بند خصم من الراتب.
/// </summary>
public class SalaryDeductionItemDto
{
    public int Id { get; set; }
    public int MonthlySalaryId { get; set; }
    public Core.Enums.DeductionType DeductionType { get; set; }
    public string DeductionTypeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}
