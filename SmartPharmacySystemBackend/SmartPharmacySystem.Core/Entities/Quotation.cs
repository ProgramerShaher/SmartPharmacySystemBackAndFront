using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// يمثل رأس عرض السعر (Price Quotation / Proforma Invoice)
/// يتيح تقديم عروض أسعار تفصيلية لعملاء الجملة والمشاريع وتجهيزات المعارض والمحلات
/// مع إمكانية تحويل العرض المقبول آلياً إلى فاتورة مبيعات بدون تكرار الإدخال.
/// </summary>
public class Quotation : BaseMultiBranchEntity
{
    /// <summary>
    /// رقم عرض السعر الفريد (مثال: QT-2026-000001)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string QuotationNumber { get; set; } = string.Empty;

    /// <summary>
    /// تاريخ إصدار عرض السعر
    /// </summary>
    public DateTime QuotationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// تاريخ انتهاء صلاحية العرض وسريان الأسعار
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// معرف العميل المسجل (اختياري للعملاء الجدد أو المحتملين)
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// العميل المرتبط بالعرض
    /// </summary>
    [ForeignKey("CustomerId")]
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// اسم العميل أو المنشأة
    /// </summary>
    [MaxLength(200)]
    public string? CustomerName { get; set; }

    /// <summary>
    /// رقم هاتف العميل للتواصل
    /// </summary>
    [MaxLength(50)]
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// البريد الإلكتروني للعميل
    /// </summary>
    [MaxLength(100)]
    public string? CustomerEmail { get; set; }

    /// <summary>
    /// حالة عرض السعر الحالية
    /// </summary>
    public QuotationStatus Status { get; set; } = QuotationStatus.Draft;

    /// <summary>
    /// المجموع الفرعي قبل الضريبة
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; } = 0;

    /// <summary>
    /// نسبة ضريبة القيمة المضافة المطبقة (%)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxRate { get; set; } = 0;

    /// <summary>
    /// إجمالي مبلغ ضريبة القيمة المضافة
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; } = 0;

    /// <summary>
    /// هل الأسعار شاملة الضريبة أم غير شاملة (مضافة)
    /// </summary>
    public bool IsTaxInclusive { get; set; } = true;

    /// <summary>
    /// إجمالي الخصم المطبق على مستوى العرض
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDiscount { get; set; } = 0;

    /// <summary>
    /// الإجمالي النهائي لعرض السعر شامل الضريبة بعد الخصم
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; } = 0;

    /// <summary>
    /// ملاحظات عامة حول العرض
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// الشروط والأحكام الخاصة بالعرض (مثل: مدة التوريد، موقع الاستلام، طريقة الدفع، الضمان)
    /// </summary>
    [MaxLength(2000)]
    public string? TermsAndConditions { get; set; }

    /// <summary>
    /// معرف فاتورة المبيعات في حال تم تحويل العرض إلى فاتورة
    /// </summary>
    public int? ConvertedSaleInvoiceId { get; set; }

    /// <summary>
    /// فاتورة المبيعات الناتجة عن التحويل
    /// </summary>
    [ForeignKey("ConvertedSaleInvoiceId")]
    public virtual SaleInvoice? ConvertedSaleInvoice { get; set; }

    /// <summary>
    /// تاريخ ووقت تحويل العرض إلى فاتورة
    /// </summary>
    public DateTime? ConvertedAt { get; set; }

    /// <summary>
    /// معرف المستخدم الذي قام بتحويل العرض إلى فاتورة
    /// </summary>
    public int? ConvertedBy { get; set; }

    /// <summary>
    /// قائمة بنود وأصناف عرض السعر
    /// </summary>
    public virtual ICollection<QuotationDetail> QuotationDetails { get; set; } = new List<QuotationDetail>();
}
