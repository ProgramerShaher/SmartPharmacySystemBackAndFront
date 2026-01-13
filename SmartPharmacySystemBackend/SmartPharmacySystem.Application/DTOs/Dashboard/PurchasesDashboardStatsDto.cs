namespace SmartPharmacySystem.Application.DTOs.Dashboard;

/// <summary>
/// DTO for Purchases Dashboard Statistics
/// Optimized for high-performance aggregate queries
/// </summary>
public class PurchasesDashboardStatsDto
{
    /// <summary>
    /// Sum of approved purchases this month
    /// </summary>
    public decimal MonthlyTotalPurchases { get; set; }

    /// <summary>
    /// Total outstanding supplier debts
    /// Sum of Supplier.Balance where Balance > 0
    /// </summary>
    public decimal SupplierDebts { get; set; }

    /// <summary>
    /// Count of overdue/pending invoices (optional business logic)
    /// </summary>
    public int OverdueCount { get; set; }

    /// <summary>
    /// Total returns amount this month
    /// </summary>
    public decimal MonthlyReturnsAmount { get; set; }

    /// <summary>
    /// Percentage: (MonthlyReturnsAmount / MonthlyTotalPurchases) * 100
    /// </summary>
    public decimal ReturnRate { get; set; }

    /// <summary>
    /// Top suppliers by purchase amount (for donut chart)
    /// </summary>
    public List<SupplierDistributionItem> SupplierDistribution { get; set; } = new();

    /// <summary>
    /// Daily purchase totals for the last 7 days (for sparkline chart)
    /// Index 0 = 6 days ago, Index 6 = Today
    /// </summary>
    public List<decimal> Last7DaysPurchases { get; set; } = new();
}

/// <summary>
/// Item for supplier distribution chart
/// </summary>
public class SupplierDistributionItem
{
    public string SupplierName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
