namespace SmartPharmacySystem.Application.DTOs.PurchaseInvoiceDetails;

/// <summary>
/// كائن نقل البيانات لتفصيل فاتورة الشراء.
/// يحتوي على جميع بيانات تفصيل فاتورة الشراء للعرض.
/// </summary>
public class PurchaseInvoiceDetailDto
{
    /// <summary>
    /// معرف التفصيل
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
    /// معرف الدواء
    /// </summary>
    public int MedicineId { get; set; }

    /// <summary>
    /// اسم الدواء
    /// </summary>
    public string MedicineName { get; set; } = string.Empty;

    /// <summary>
    /// معرف دفعة الدواء
    /// </summary>
    public int BatchId { get; set; }

    /// <summary>
    /// رقم دفعة الشركة
    /// </summary>
    public string CompanyBatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// الكمية
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// سعر الشراء للوحدة
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// المجموع الكلي
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// هل محذوف
    /// </summary>
    public bool IsDeleted { get; set; }
}