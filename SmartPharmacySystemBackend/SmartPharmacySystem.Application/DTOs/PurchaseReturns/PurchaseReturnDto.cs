namespace SmartPharmacySystem.Application.DTOs.PurchaseReturns;

/// <summary>
/// كائن نقل البيانات لإرجاع الشراء.
/// يحتوي على جميع بيانات إرجاع الشراء للعرض.
/// </summary>
public class PurchaseReturnDto
{
    /// <summary>
    /// معرف الإرجاع
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// معرف فاتورة الشراء
    /// </summary>
    public int PurchaseInvoiceId { get; set; }

    /// <summary>
    /// رقم فاتورة الشراء
    /// </summary>
    public string PurchaseInvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// معرف المورد
    /// </summary>
    public int SupplierId { get; set; }

    /// <summary>
    /// اسم المورد
    /// </summary>
    public string SupplierName { get; set; } = string.Empty;

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
    public SmartPharmacySystem.Core.Enums.DocumentStatus Status { get; set; }

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
    /// تفاصيل المرتجع (الأصناف)
    /// </summary>
    public List<PurchaseReturnDetails.PurchaseReturnDetailDto> Items { get; set; } = new();
}