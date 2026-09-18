using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Application.DTOs.SalesInvoices;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Quotations;

/// <summary>
/// كائن طلب تحويل عرض السعر إلى فاتورة مبيعات
/// </summary>
public class ConvertQuotationToInvoiceDto
{
    [Required(ErrorMessage = "يرجى تحديد طريقة الدفع")]
    public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;

    /// <summary>
    /// المبلغ المدفوع (مطلوب للنقدي / الشبكة / الدفع الجزئي)
    /// </summary>
    public decimal? PaidAmount { get; set; }

    /// <summary>
    /// في حال رغبة المستخدم في تحديد فرع مختلف أو استخدام فرع العرض الافتراضي
    /// </summary>
    public int? BranchId { get; set; }

    /// <summary>
    /// ملاحظات إضافية على الفاتورة المنشأة
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// توزيع الدفع المتعدد (نقدي + شبكة + آجل...)
    /// </summary>
    public List<CreateSaleInvoicePaymentDto>? Payments { get; set; }
}
