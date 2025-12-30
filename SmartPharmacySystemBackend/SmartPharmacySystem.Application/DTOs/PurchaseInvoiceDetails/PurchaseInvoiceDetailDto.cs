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
    /// الكمية المجانية (Bonus)
    /// </summary>
    public int BonusQuantity { get; set; }

    /// <summary>
    /// سعر الشراء للوحدة
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// سعر البيع للوحدة
    /// </summary>
    public decimal SalePrice { get; set; }

    /// <summary>
    /// الإجمالي لهذا الصنف
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// سعر التكلفة الحقيقي (بعد المجاني)
    /// </summary>
    public decimal TrueUnitCost { get; set; }

    /// <summary>
    /// هل محذوف
    /// </summary>
    public bool IsDeleted { get; set; }

    // --- Calculated / Extra Fields ---

    /// <summary>
    /// تاريخ الانتهاء (من الدفعة)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// الأيام المتبقية للصلاحية
    /// </summary>
    public int DaysUntilExpiry { get; set; }

    /// <summary>
    /// هل يمكن بيعه (قابل للصرف)
    /// </summary>
    public bool CanSell { get; set; }

    /// <summary>
    /// حالة الصلاحية (صالح / قريب الانتهاء / منتهي)
    /// </summary>
    public string ExpiryStatus { get; set; } = string.Empty;
}