using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

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
    //[StringLength(50, MinimumLength = 1, ErrorMessage = "طريقة الدفع يجب أن تكون بين 1 و 50 حرف")]
    public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;

    /// <summary>
    /// معرف العميل
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// تفاصيل الفاتورة (قائمة الأصناف الجديدة)
    /// </summary>
    public List<SalesInvoiceDetails.CreateSaleInvoiceDetailDto> Details { get; set; } = new();
}