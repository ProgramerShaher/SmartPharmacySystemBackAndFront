using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.SalesReturns;

/// <summary>
/// كائن نقل البيانات لإرجاع البيع.
/// يحتوي على جميع بيانات إرجاع البيع للعرض.
/// </summary>
public class SalesReturnDto
{
    /// <summary>
    /// معرف الإرجاع
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// معرف فاتورة البيع
    /// </summary>
    public int SaleInvoiceId { get; set; }

    /// <summary>
    /// رقم فاتورة البيع
    /// </summary>
    public string SaleInvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// تاريخ الإرجاع
    /// </summary>
    public DateTime ReturnDate { get; set; }

    /// <summary>
    /// المبلغ الإجمالي
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// سبب الإرجاع
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// اسم العميل
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// معرف المستخدم الذي أنشأ الإرجاع
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
    /// حالة المرتجع
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
    /// تفاصيل المرتجع
    /// </summary>
    public List<SalesReturnDetails.SalesReturnDetailDto> Items { get; set; } = new();
}