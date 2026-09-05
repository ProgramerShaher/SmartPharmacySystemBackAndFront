using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.PurchaseInvoiceDetails;

/// <summary>
/// كائن نقل البيانات لتحديث تفصيل فاتورة شراء.
/// يحتوي على البيانات القابلة للتحديث لتفصيل فاتورة الشراء.
/// </summary>
public class UpdatePurchaseInvoiceDetailDto
{
    /// <summary>
    /// معرف تفصيل فاتورة الشراء
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// معرف فاتورة الشراء
    /// </summary>
    [Required]
    public int PurchaseInvoiceId { get; set; }

    /// <summary>
    /// معرف الدواء
    /// </summary>
    [Required]
    public int MedicineId { get; set; }

    /// <summary>
    /// معرف دفعة الدواء
    /// </summary>
    [Required]
    public int BatchId { get; set; }

    /// <summary>
    /// الكمية
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
    public int Quantity { get; set; }

    /// <summary>
    /// سعر الشراء للوحدة
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "سعر الشراء يجب أن يكون أكبر من صفر")]
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// سعر البيع المقترح للوحدة
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "سعر البيع يجب أن يكون أكبر من صفر")]
    public decimal SalePrice { get; set; }

    /// <summary>
    /// تاريخ انتهاء الصلاحية
    /// </summary>
    [Required]
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// باركود الدفعة
    /// </summary>
    public string? BatchBarcode { get; set; }

    /// <summary>
    /// رقم تشغيلة الشركة المصنعة
    /// </summary>
    public string? CompanyBatchNumber { get; set; }

    /// <summary>
    /// موقع التخزين (اختياري)
    /// </summary>
    public string? StorageLocation { get; set; }

    public int? PurchaseUnitId { get; set; }
    public int BonusQuantity { get; set; }
}
