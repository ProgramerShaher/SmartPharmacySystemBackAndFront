using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.DailyClosings;

/// <summary>
/// كائن نقل البيانات لعرض إغلاق يومي لفرع.
/// </summary>
public class DailyClosingDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime ClosingDate { get; set; }
    public ClosingStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public decimal OpeningCash { get; set; }
    public decimal TotalCashSales { get; set; }
    public decimal TotalCreditSales { get; set; }
    public decimal TotalCardSales { get; set; }
    public decimal TotalCollections { get; set; }
    public decimal TotalCashReturns { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal ActualCash { get; set; }
    public decimal CashVariance { get; set; }
    public int SubmittedByUserId { get; set; }
    public string SubmittedByName { get; set; } = string.Empty;
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
