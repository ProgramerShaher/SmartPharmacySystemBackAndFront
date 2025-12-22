using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.SalesInvoices;

/// <summary>
/// كائن نقل البيانات لتحديث فاتورة بيع.
/// يحتوي على البيانات القابلة للتحديث لفاتورة البيع.
/// </summary>
public class UpdateSaleInvoiceDto
{
    /// <summary>
    /// معرف الفاتورة
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// تاريخ فاتورة البيع
    /// </summary>
    [Required]
    public DateTime SaleInvoiceDate { get; set; }

    /// <summary>
    /// طريقة الدفع
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "طريقة الدفع يجب أن تكون بين 1 و 50 حرف")]
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// اسم العميل
    /// </summary>
    [StringLength(100, ErrorMessage = "اسم العميل يجب أن يكون أقل من 100 حرف")]
    public string CustomerName { get; set; } = string.Empty;
}