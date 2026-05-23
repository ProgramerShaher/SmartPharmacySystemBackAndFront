namespace SmartPharmacySystem.Application.DTOs.DailyClosings;

/// <summary>
/// كائن نقل البيانات لعرض ملخص إغلاق يومي لجميع الفروع.
/// </summary>
public class DailyClosingSummaryDto
{
    public DateTime ClosingDate { get; set; }
    public List<DailyClosingDto> BranchClosings { get; set; } = new();
    public decimal TotalCashSales { get; set; }
    public decimal TotalCreditSales { get; set; }
    public decimal TotalCardSales { get; set; }
    public decimal TotalCollections { get; set; }
    public decimal TotalCashReturns { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal GrandTotalRevenue { get; set; }
    public int TotalBranchesClosed { get; set; }
    public int TotalBranchesPending { get; set; }
}
