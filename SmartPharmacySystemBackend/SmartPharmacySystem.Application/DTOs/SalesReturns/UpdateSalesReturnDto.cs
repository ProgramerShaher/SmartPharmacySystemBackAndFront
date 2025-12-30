using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.SalesReturns;

/// <summary>
/// كائن نقل البيانات لتحديث إرجاع بيع.
/// يحتوي على البيانات القابلة للتحديث لإرجاع البيع.
/// </summary>
public class UpdateSalesReturnDto
{
    /// <summary>
    /// معرف إرجاع البيع
    /// </summary>
    [Required]
    public int Id { get; set; }

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

    /// <summary>
    /// تفاصيل المرتجع (قائمة الأصناف الجديدة)
    /// </summary>
    public List<SalesReturnDetails.CreateSalesReturnDetailDto> Details { get; set; } = new();
}
