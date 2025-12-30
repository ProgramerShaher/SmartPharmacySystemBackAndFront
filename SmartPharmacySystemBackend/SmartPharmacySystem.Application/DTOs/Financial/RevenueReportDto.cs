namespace SmartPharmacySystem.Application.DTOs.Financial;

/// <summary>
/// DTO لتقرير الإيرادات
/// Revenue report DTO
/// </summary>
public class RevenueReportDto
{
    /// <summary>
    /// إجمالي الإيرادات
    /// Total revenue
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// إيرادات المبيعات
    /// Sales revenue
    /// </summary>
    public decimal SalesRevenue { get; set; }

    /// <summary>
    /// إيرادات مرتجعات المشتريات
    /// Purchase returns revenue
    /// </summary>
    public decimal PurchaseReturnsRevenue { get; set; }

    /// <summary>
    /// الإيرادات حسب طريقة الدفع
    /// Revenue by payment method
    /// </summary>
    public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; } = new();

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
