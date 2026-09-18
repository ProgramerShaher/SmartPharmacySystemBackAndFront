using SmartPharmacySystem.Core.Enums;
using System.Collections.Generic;

namespace SmartPharmacySystem.Application.DTOs.SalesInvoices;

/// <summary>
/// كائن نقل البيانات لفاتورة البيع.
/// يحتوي على جميع بيانات فاتورة البيع للعرض.
/// </summary>
public class SaleInvoiceDto
{
    /// <summary>
    /// معرف الفاتورة
    /// </summary>
    public int Id { get; set; }

    public int? BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// تاريخ فاتورة البيع
    /// </summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>
    /// رقم فاتورة المبيعات
    /// </summary>
    public string SaleInvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// المبلغ الإجمالي (شاملاً الضريبة)
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// المجموع الفرعي قبل الضريبة
    /// </summary>
    public decimal Subtotal { get; set; } = 0;

    /// <summary>
    /// نسبة ضريبة القيمة المضافة (%)
    /// </summary>
    public decimal TaxRate { get; set; } = 0;

    /// <summary>
    /// إجمالي مبلغ الضريبة
    /// </summary>
    public decimal TaxAmount { get; set; } = 0;

    /// <summary>
    /// هل الأسعار شاملة الضريبة
    /// </summary>
    public bool IsTaxInclusive { get; set; } = true;

    /// <summary>
    /// رمز الاستجابة السريعة المشفر وفق متطلبات هيئة الزكاة والضريبة والجمارك (ZATCA Base64)
    /// </summary>
    public string? ZatcaQrCode { get; set; }

    /// <summary>
    /// إجمالي الخصم المطبق على الفاتورة
    /// </summary>
    public decimal TotalDiscount { get; set; } = 0;

    /// <summary>
    /// المبلغ المدفوع
    /// </summary>
    public decimal PaidAmount { get; set; } = 0;

    /// <summary>
    /// المبلغ المتبقي
    /// </summary>
    public decimal RemainingAmount => Math.Max(0, TotalAmount - PaidAmount);

    /// <summary>
    /// هل تم سداد الفاتورة بالكامل
    /// </summary>
    public bool IsPaid { get; set; } = false;

    /// <summary>
    /// التكلفة الإجمالية
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// الربح الإجمالي
    /// </summary>
    public decimal TotalProfit { get; set; }

    /// <summary>
    /// طريقة الدفع
    /// </summary>
    public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;

    /// <summary>
    /// معرف العميل
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// اسم العميل
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// معرف المستخدم الذي أنشأ الفاتورة
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// تاريخ الإنشاء
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public string CreatedByName { get; set; } = string.Empty;
    public string? ApprovedByName { get; set; }
    public string? CancelledByName { get; set; }

    /// <summary>
    /// حالة الفاتورة
    /// </summary>
    public DocumentStatus Status { get; set; }

    // Status Tracking & Dynamic Colors
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public string StatusIcon { get; set; } = string.Empty;

    // Action Tracking (Last Action)
    public string ActionByName { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }

    /// <summary>
    /// تفاصيل الفاتورة
    /// </summary>
    public List<SalesInvoiceDetails.SaleInvoiceDetailDto> Items { get; set; } = new();

    /// <summary>
    /// سجل دفعات الفاتورة (الدفع المتعدد)
    /// </summary>
    public List<SaleInvoicePaymentDto> Payments { get; set; } = new();
}
