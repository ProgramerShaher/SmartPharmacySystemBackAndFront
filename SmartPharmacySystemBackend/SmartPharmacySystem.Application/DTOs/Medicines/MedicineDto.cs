namespace SmartPharmacySystem.Application.DTOs.Medicine;

/// <summary>
/// كائن نقل البيانات للدواء.
/// يحتوي على جميع بيانات الدواء للعرض.
/// </summary>
public class MedicineDto
{
    /// <summary>
    /// معرف الدواء
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// الكود الداخلي
    /// </summary>
    public string InternalCode { get; set; } = string.Empty;

    /// <summary>
    /// اسم الدواء
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// معرف الفئة
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// اسم الفئة
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// الشركة المصنعة
    /// </summary>
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// الباركود الافتراضي
    /// </summary>
    public string DefaultBarcode { get; set; } = string.Empty;

    /// <summary>
    /// سعر الشراء الافتراضي
    /// </summary>
    public decimal DefaultPurchasePrice { get; set; }

    /// <summary>
    /// سعر البيع الافتراضي
    /// </summary>
    public decimal DefaultSalePrice { get; set; }

    /// <summary>
    /// الحد الأدنى للتنبيه بالكمية
    /// </summary>
    public int MinAlertQuantity { get; set; }

    /// <summary>
    /// هل يُباع بالوحدة
    /// </summary>
    public bool SoldByUnit { get; set; }

    /// <summary>
    /// الحالة
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// ملاحظات
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// تاريخ الإنشاء
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// هل محذوف
    /// </summary>
    public bool IsDeleted { get; set; }
}