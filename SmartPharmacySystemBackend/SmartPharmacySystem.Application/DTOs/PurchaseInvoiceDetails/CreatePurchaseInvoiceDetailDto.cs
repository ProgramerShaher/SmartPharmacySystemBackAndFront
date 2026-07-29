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
    /// الكمية كما يدخلها المستخدم بالوحدة المختارة (مثلاً: 5 كراتين).
    /// Quantity as entered by the user in the selected purchase unit (e.g. 5 cartons).
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
    public int Quantity { get; set; }  // kept for backward-compat; equals QuantityInPurchaseUnit when sent

    /// <summary>
    /// معرف وحدة الشراء (مثلاً: كرتون). إذا كان null سيتم التعامل بالوحدة الأساسية (x1).
    /// FK to MedicineUnit. If null, base unit (conversion factor = 1) is assumed.
    /// </summary>
    public int? PurchaseUnitId { get; set; }

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

    /// <summary>
    /// موقع التخزين (اختياري)
    /// </summary>
    public string? StorageLocation { get; set; }
}