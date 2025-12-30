using SmartPharmacySystem.Application.DTOs.Shared;

namespace SmartPharmacySystem.Application.DTOs.Expense;

public class ExpenseQueryDto : BaseQueryDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? CategoryId { get; set; }
    public string? ExpenseType { get; set; } // Compatibility with existing frontend
}
