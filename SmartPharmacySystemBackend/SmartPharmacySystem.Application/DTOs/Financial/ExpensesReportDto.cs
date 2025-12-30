using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Financial;

/// <summary>
/// DTO لتقرير المصروفات
/// Expenses report DTO
/// </summary>
public class ExpensesReportDto
{
    /// <summary>
    /// إجمالي المصروفات
    /// Total expenses
    /// </summary>
    public decimal TotalExpenses { get; set; }

    /// <summary>
    /// المصروفات حسب النوع
    /// Expenses by type
    /// </summary>
    public Dictionary<string, decimal> ExpensesByType { get; set; } = new();

    /// <summary>
    /// المصروفات حسب طريقة الدفع
    /// Expenses by payment method
    /// </summary>
    public Dictionary<string, decimal> ExpensesByPaymentMethod { get; set; } = new();

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
