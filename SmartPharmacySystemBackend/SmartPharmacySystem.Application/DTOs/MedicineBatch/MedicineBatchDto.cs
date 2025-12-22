namespace SmartPharmacySystem.Application.DTOs.MedicineBatch;

/// <summary>
/// كائن نقل البيانات لدفعة الدواء.
/// يحتوي على جميع بيانات دفعة الدواء للعرض.
/// </summary>
public class MedicineBatchDto
{
    /// <summary>
    /// معرف الدفعة
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// الكود الرسمي للشركة (المعرف التجاري الرئيسي)
    /// </summary>
    public string CompanyBatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// معرف الدواء
    /// </summary>
    public int MedicineId { get; set; }

    /// <summary>
    /// اسم الدواء
    /// </summary>
    public string MedicineName { get; set; } = string.Empty;

    /// <summary>
    /// تاريخ انتهاء الصلاحية
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// الكمية
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// الكمية المتبقية
    /// </summary>
    public int RemainingQuantity { get; set; }

    /// <summary>
    /// سعر الشراء للوحدة
    /// </summary>
    public decimal UnitPurchasePrice { get; set; }

    /// <summary>
    /// باركود الدفعة
    /// </summary>
    public string BatchBarcode { get; set; } = string.Empty;

    /// <summary>
    /// الحالة
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// مكان التخزين
    /// </summary>
    public string StorageLocation { get; set; } = string.Empty;

    /// <summary>
    /// تاريخ الدخول
    /// </summary>
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// معرف المستخدم الذي أنشأ السجل
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// هل محذوف
    /// </summary>
    public bool IsDeleted { get; set; }
}