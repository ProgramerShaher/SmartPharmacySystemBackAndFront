using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.PurchaseReturns;

/// <summary>
/// كائن نقل البيانات لتحديث إرجاع شراء.
/// يحتوي على البيانات القابلة للتحديث لإرجاع الشراء.
/// </summary>
public class UpdatePurchaseReturnDto
{
    /// <summary>
    /// معرف إرجاع الشراء
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// معرف فاتورة الشراء
    /// </summary>
    [Required]
    public int PurchaseInvoiceId { get; set; }

    /// <summary>
    /// معرف المورد
    /// </summary>
    [Required]
    public int SupplierId { get; set; }

    /// <summary>
    /// تاريخ الإرجاع
    /// </summary>
    [Required]
    public DateTime ReturnDate { get; set; }

    /// <summary>
    /// سبب الإرجاع
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "سبب الإرجاع يجب أن يكون بين 1 و 500 حرف")]
    public string Reason { get; set; } = string.Empty;
}
