using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.SalesReturns;

/// <summary>
/// كائن نقل البيانات لإنشاء إرجاع بيع جديد.
/// يحتوي على البيانات المطلوبة لإنشاء إرجاع بيع.
/// </summary>
public class CreateSalesReturnDto
{
    /// <summary>
    /// معرف فاتورة البيع
    /// </summary>
    [Required]
    public int SaleInvoiceId { get; set; }

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