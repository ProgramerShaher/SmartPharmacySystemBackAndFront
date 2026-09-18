using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// يمثل رأس أمر الشراء (Purchase Order - PO)
/// يتيح إصدار طلبات توريد وبضاعة رسمية للموردين بأسعار وشروط محددة
/// مع إمكانية تحويل الأمر عند التوريد آلياً إلى فاتورة مشتريات حقيقية تدخل المخزون وتحدث الحسابات.
/// </summary>
public class PurchaseOrder : BaseMultiBranchEntity
{
    /// <summary>
    /// رقم أمر الشراء الفريد (مثال: PO-2026-000001)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// تاريخ إصدار أمر الشراء
    /// </summary>
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// تاريخ التوريد والتسليم المتوقع من المورد
    /// </summary>
    public DateTime? ExpectedDeliveryDate { get; set; }

    /// <summary>
    /// معرف المورد المسجل
    /// </summary>
    public int? SupplierId { get; set; }

    /// <summary>
    /// المورد المرتبط بأمر الشراء
    /// </summary>
    [ForeignKey("SupplierId")]
    public virtual Supplier? Supplier { get; set; }

    /// <summary>
    /// اسم المورد أو الجهة الموردة
    /// </summary>
    [MaxLength(200)]
    public string? SupplierName { get; set; }

    /// <summary>
    /// بيانات التواصل مع المورد (هاتف/بريد)
    /// </summary>
    [MaxLength(100)]
    public string? SupplierContact { get; set; }

    /// <summary>
    /// حالة أمر الشراء الحالية
    /// </summary>
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    /// <summary>
    /// المجموع الفرعي قبل الضريبة والخصم
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
    /// هل الأسعار شاملة الضريبة
    /// </summary>
    public bool IsTaxInclusive { get; set; } = false;

    /// <summary>
    /// إجمالي الخصم المطبق على مستوى أمر الشراء
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDiscount { get; set; } = 0;

    /// <summary>
    /// تكلفة الشحن والنقل المتوقعة
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ShippingCost { get; set; } = 0;

    /// <summary>
    /// الإجمالي النهائي لأمر الشراء
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; } = 0;

    /// <summary>
    /// ملاحظات عامة حول أمر الشراء
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// الشروط والأحكام الخاصة بأمر الشراء (طريقة السداد، مكان التسليم، شروط الفحص)
    /// </summary>
    [MaxLength(2000)]
    public string? TermsAndConditions { get; set; }

    /// <summary>
    /// معرف فاتورة المشتريات الناتجة عن تحويل هذا الأمر
    /// </summary>
    public int? ConvertedPurchaseInvoiceId { get; set; }

    /// <summary>
    /// فاتورة المشتريات المرتبطة
    /// </summary>
    [ForeignKey("ConvertedPurchaseInvoiceId")]
    public virtual PurchaseInvoice? ConvertedPurchaseInvoice { get; set; }

    /// <summary>
    /// تاريخ تحويل أمر الشراء إلى فاتورة
    /// </summary>
    public DateTime? ConvertedAt { get; set; }

    /// <summary>
    /// معرف المستخدم الذي قام بالتحويل
    /// </summary>
    public int? ConvertedBy { get; set; }

    /// <summary>
    /// بنود وتفاصيل أمر الشراء
    /// </summary>
    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();
}
