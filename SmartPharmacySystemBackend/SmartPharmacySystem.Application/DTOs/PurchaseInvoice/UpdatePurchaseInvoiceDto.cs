using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.CreatePurchaseInvoice;

/// <summary>
/// كائن نقل البيانات لتحديث فاتورة شراء.
/// يحتوي على البيانات القابلة للتحديث لفاتورة الشراء.
/// </summary>
public class UpdatePurchaseInvoiceDto
{
    /// <summary>
    /// معرف فاتورة الشراء
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// معرف المورد
    /// </summary>
    [Required]
    public int SupplierId { get; set; }

    /// <summary>
    /// رقم فاتورة المورد
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "رقم فاتورة المورد يجب أن يكون بين 1 و 100 حرف")]
    public string SupplierInvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// تاريخ الشراء
    /// </summary>
    [Required]
    public DateTime PurchaseDate { get; set; }

    /// <summary>
    /// طريقة الدفع
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "طريقة الدفع يجب أن تكون بين 1 و 50 حرف")]
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// ملاحظات
    /// </summary>
    [StringLength(500, ErrorMessage = "الملاحظات يجب أن تكون أقل من 500 حرف")]
    public string Notes { get; set; } = string.Empty;
}
