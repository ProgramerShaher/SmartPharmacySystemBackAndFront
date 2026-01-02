using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.SalesInvoices;

/// <summary>
/// كائن نقل البيانات لإنشاء فاتورة بيع جديدة.
/// يحتوي على البيانات المطلوبة لإنشاء فاتورة بيع.
/// </summary>
public class CreateSaleInvoiceDto
{
    /// <summary>
    /// تاريخ فاتورة البيع
    /// </summary>
    [Required]
    public DateTime InvoiceDate { get; set; }

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
    /// اسم العميل (مطلوب للزبون الطيار)
    /// Customer name (required for walk-in customers)
    /// </summary>
    [StringLength(200)]
    public string? CustomerName { get; set; }

    /// <summary>
    /// تفاصيل الفاتورة
    /// </summary>
    public List<SalesInvoiceDetails.CreateSaleInvoiceDetailDto> Details { get; set; } = new();


}
