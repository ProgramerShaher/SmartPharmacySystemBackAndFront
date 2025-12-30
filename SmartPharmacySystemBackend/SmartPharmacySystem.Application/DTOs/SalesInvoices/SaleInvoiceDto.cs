using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.SalesInvoices;

/// <summary>
/// كائن نقل البيانات لفاتورة البيع.
/// يحتوي على جميع بيانات فاتورة البيع للعرض.
/// </summary>
public class SaleInvoiceDto
{
    /// <summary>
    /// معرف الفاتورة
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// تاريخ فاتورة البيع
    /// </summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>
    /// رقم فاتورة المبيعات
    /// </summary>
    public string SaleInvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// المبلغ الإجمالي
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// التكلفة الإجمالية
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// الربح الإجمالي
    /// </summary>
    public decimal TotalProfit { get; set; }

    /// <summary>
    /// طريقة الدفع
    /// </summary>
    public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;

    /// <summary>
    /// اسم العميل
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// معرف المستخدم الذي أنشأ الفاتورة
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// تاريخ الإنشاء
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public string CreatedByName { get; set; } = string.Empty;
    public string? ApprovedByName { get; set; }
    public string? CancelledByName { get; set; }

    /// <summary>
    /// هل محذوف
    /// </summary>
    /// <summary>
    /// حالة الفاتورة
    /// </summary>
    public DocumentStatus Status { get; set; }

    // Status Tracking & Dynamic Colors
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public string StatusIcon { get; set; } = string.Empty;

    // Action Tracking (Last Action)
    public string ActionByName { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }

    /// <summary>
    /// تفاصيل الفاتورة
    /// </summary>
    public List<SalesInvoiceDetails.SaleInvoiceDetailDto> Items { get; set; } = new();
}