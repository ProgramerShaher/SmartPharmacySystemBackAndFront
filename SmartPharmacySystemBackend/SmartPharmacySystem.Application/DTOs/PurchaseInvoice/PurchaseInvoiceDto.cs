using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.CreatePurchaseInvoice;

/// <summary>
/// كائن نقل البيانات لفاتورة الشراء.
/// يحتوي على جميع بيانات فاتورة الشراء للعرض.
/// </summary>
public class PurchaseInvoiceDto
{
    /// <summary>
    /// معرف الفاتورة
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// معرف المورد
    /// </summary>
    public int SupplierId { get; set; }

    /// <summary>
    /// اسم المورد
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;

    /// <summary>
    /// رقم فاتورة المورد
    /// </summary>
    public string SupplierInvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// رقم فاتورة الشراء الداخلي
    /// </summary>
    public string PurchaseInvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// تاريخ الشراء
    /// </summary>
    public DateTime PurchaseDate { get; set; }

    /// <summary>
    /// المبلغ الإجمالي
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// طريقة الدفع
    /// </summary>
    public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;

    /// <summary>
    /// ملاحظات
    /// </summary>
    public string Notes { get; set; } = string.Empty;

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
    public DocumentStatus Status { get; set; }

    // Status Tracking & Dynamic Colors
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public string StatusIcon { get; set; } = string.Empty;

    // Action Tracking (Last Action)
    public string ActionByName { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }

    /// <summary>
    /// هل محذوف
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تفاصيل الفاتورة (الأصناف)
    /// </summary>
    public List<PurchaseInvoiceDetails.PurchaseInvoiceDetailDto> Items { get; set; } = new();
}