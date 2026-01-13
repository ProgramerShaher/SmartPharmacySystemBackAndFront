namespace SmartPharmacySystem.Application.DTOs.Dashboard;

/// <summary>
/// DTO for Sales Dashboard Statistics
/// Optimized for high-performance aggregate queries
/// </summary>
public class SalesDashboardStatsDto
{
    /// <summary>
    /// Sum of approved sales today
    /// </summary>
    public decimal TodayTotalSales { get; set; }

    /// <summary>
    /// Sum of TotalProfit for approved sales today
    /// Calculated as (SalePrice - BatchCost) * Quantity
    /// </summary>
    public decimal TodayNetProfit { get; set; }

    /// <summary>
    /// Total outstanding customer debts
    /// Sum of Customer.Balance where Balance > 0
    /// </summary>
    public decimal CustomerDebts { get; set; }

    /// <summary>
    /// Sum of approved sales returns today
    /// </summary>
    public decimal TodayReturnsAmount { get; set; }

    /// <summary>
    /// Percentage: (TodayReturnsAmount / TodayTotalSales) * 100
    /// </summary>
    public decimal ReturnRate { get; set; }

    /// <summary>
    /// Daily sales totals for the last 7 days (for sparkline chart)
    /// Index 0 = 6 days ago, Index 6 = Today
    /// </summary>
    public List<decimal> Last7DaysSales { get; set; } = new();

    /// <summary>
    /// Cash percentage of today's sales
    /// </summary>
    public decimal CashPercentage { get; set; }
}
