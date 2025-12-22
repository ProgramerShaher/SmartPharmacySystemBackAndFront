using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.MedicineBatch;

/// <summary>
/// كائن نقل البيانات لتحديث دفعة دواء.
/// يحتوي على البيانات القابلة للتحديث لدفعة الدواء.
/// </summary>
public class UpdateMedicineBatchDto
{
    /// <summary>
    /// معرف الدفعة
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// معرف الدواء
    /// </summary>
    [Required]
    public int MedicineId { get; set; }

    /// <summary>
    /// الكود الرسمي للشركة (مطلوب)
    /// </summary>
    [Required(ErrorMessage = "الكود الرسمي للشركة مطلوب")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "الكود الرسمي للشركة يجب أن يكون بين 1 و 100 حرف")]
    public string CompanyBatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// الكمية
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// تاريخ انتهاء الصلاحية
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// تاريخ الشراء
    /// </summary>
    public DateTime PurchaseDate { get; set; }

    /// <summary>
    /// سعر الشراء
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// سعر البيع
    /// </summary>
    public decimal SalePrice { get; set; }

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
}
