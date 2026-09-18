using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;
using System;
using System.Collections.Generic;

namespace SmartPharmacySystem.Application.DTOs.CreatePurchaseInvoice;

/// <summary>
/// كائن نقل البيانات لإنشاء فاتورة شراء جديدة.
/// يحتوي على البيانات المطلوبة لإنشاء فاتورة شراء.
/// </summary>
public class CreatePurchaseInvoiceDto
{
    /// <summary>
    /// معرف المورد
    /// </summary>
    public int? SupplierId { get; set; }

    /// <summary>
    /// رقم فاتورة المورد
    /// </summary>
    [StringLength(100, MinimumLength = 1, ErrorMessage = "رقم فاتورة المورد يجب أن يكون بين 1 و 100 حرف")]
    public string? SupplierInvoiceNumber { get; set; }

    /// <summary>
    /// تاريخ الشراء
    /// </summary>
    [Required]
    public DateTime PurchaseDate { get; set; }

    /// <summary>
    /// طريقة الدفع
    /// </summary>
    [Required]
    [EnumDataType(typeof(PaymentType))]
    public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;

    public int? WarehouseId { get; set; }

    /// <summary>
    /// ملاحظات
    /// </summary>
    [StringLength(500, ErrorMessage = "الملاحظات يجب أن تكون أقل من 500 حرف")]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// تكاليف الشحن والنقل للمشتريات
    /// </summary>
    public decimal ShippingCost { get; set; } = 0;

    /// <summary>
    /// رسوم الجمارك والتخليص
    /// </summary>
    public decimal CustomsCost { get; set; } = 0;

    /// <summary>
    /// مصاريف إضافية أخرى (تأمين، عمالة وتنزيل)
    /// </summary>
    public decimal OtherLandedCosts { get; set; } = 0;

    /// <summary>
    /// نسبة ضريبة القيمة المضافة المطبقة (%)
    /// </summary>
    public decimal TaxRate { get; set; } = 0;

    /// <summary>
    /// هل الأسعار شاملة الضريبة
    /// </summary>
    public bool IsTaxInclusive { get; set; } = true;

    /// <summary>
    /// قائمة أصناف الفاتورة
    /// List of invoice items
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "يجب إضافة صنف واحد على الأقل للفاتورة")]
    public List<SmartPharmacySystem.Application.DTOs.PurchaseInvoiceDetails.CreatePurchaseInvoiceDetailDto> Items { get; set; } = new();
}
