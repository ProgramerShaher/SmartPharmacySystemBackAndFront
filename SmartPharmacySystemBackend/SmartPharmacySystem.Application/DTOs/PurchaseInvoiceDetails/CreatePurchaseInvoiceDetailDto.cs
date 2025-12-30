using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.PurchaseInvoiceDetails;

/// <summary>
/// كائن نقل البيانات لإنشاء تفصيل فاتورة شراء جديد.
/// يحتوي على البيانات المطلوبة لإنشاء تفصيل فاتورة شراء.
/// </summary>
public class CreatePurchaseInvoiceDetailDto
{
    /// <summary>
    /// معرف الدواء
    /// </summary>
    [Required]
    public int MedicineId { get; set; }

    /// <summary>
    /// الكمية
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
    public int Quantity { get; set; }

    /// <summary>
    /// الكمية المجانية (Bonus)
    /// </summary>
    public int BonusQuantity { get; set; }

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
    /// باركود الدفعة (لتمييز الدفعات المختلفة لنفس الدواء)
    /// </summary>
    public string? BatchBarcode { get; set; }

    /// <summary>
    /// رقم تشغيلة الشركة المصنعة
    /// </summary>
    public string? CompanyBatchNumber { get; set; }
}