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

    /// <summary>
    /// هل محذوف
    /// </summary>
    public bool IsDeleted { get; set; }
}