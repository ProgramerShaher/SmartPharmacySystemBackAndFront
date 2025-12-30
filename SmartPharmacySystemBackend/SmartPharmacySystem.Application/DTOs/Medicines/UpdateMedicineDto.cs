namespace SmartPharmacySystem.Application.DTOs.Medicine;

/// <summary>
/// كائن نقل البيانات لتحديث دواء.
/// يحتوي على البيانات القابلة للتحديث للدواء.
/// </summary>
public class UpdateMedicineDto
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
    /// اسم الدواء (الاسم التجاري)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// الاسم العلمي
    /// </summary>
    public string? ScientificName { get; set; }

    /// <summary>
    /// المادة الفعالة
    /// </summary>
    public string? ActiveIngredient { get; set; }

    /// <summary>
    /// معرف الفئة
    /// </summary>
    public int? CategoryId { get; set; }

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
    /// نقطة إعادة الطلب
    /// </summary>
    public int ReorderLevel { get; set; }

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
}