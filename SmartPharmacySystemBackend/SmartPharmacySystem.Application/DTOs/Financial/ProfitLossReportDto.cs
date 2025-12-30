namespace SmartPharmacySystem.Application.DTOs.Financial;

/// <summary>
/// DTO لتقرير الأرباح والخسائر
/// Profit and loss report DTO
/// </summary>
public class ProfitLossReportDto
{
    /// <summary>
    /// إجمالي الإيرادات
    /// Total revenue
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// إجمالي التكاليف
    /// Total costs
    /// </summary>
    public decimal TotalCosts { get; set; }

    /// <summary>
    /// إجمالي المصروفات
    /// Total expenses
    /// </summary>
    public decimal TotalExpenses { get; set; }

    /// <summary>
    /// صافي الربح
    /// Net profit
    /// </summary>
    public decimal NetProfit { get; set; }

    /// <summary>
    /// هامش الربح
    /// Profit margin percentage
    /// </summary>
    public decimal ProfitMargin { get; set; }

    /// <summary>
    /// تاريخ البداية
    /// Start date
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// تاريخ النهاية
    /// End date
    /// </summary>
    public DateTime? To { get; set; }
}
