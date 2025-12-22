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
    public DateTime SaleInvoiceDate { get; set; }

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
    public string PaymentMethod { get; set; } = string.Empty;

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

    /// <summary>
    /// هل محذوف
    /// </summary>
    public bool IsDeleted { get; set; }
}