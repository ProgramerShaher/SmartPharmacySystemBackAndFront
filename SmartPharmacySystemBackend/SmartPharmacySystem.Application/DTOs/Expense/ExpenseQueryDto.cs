using SmartPharmacySystem.Application.DTOs.Shared;

namespace SmartPharmacySystem.Application.DTOs.Expense;

public class ExpenseQueryDto : BaseQueryDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? ExpenseType { get; set; }
}
