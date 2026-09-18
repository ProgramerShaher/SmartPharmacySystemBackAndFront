using SmartPharmacySystem.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a sale invoice in the pharmacy system.
/// Sale invoices record medicine sales to customers.
/// </summary>
public class SaleInvoice : BaseMultiBranchEntity
{

    /// <summary>
    /// Unique number for the sale invoice (Format: SI-YYYY-######).
    /// </summary>
    public string SaleInvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Date of the sale.
    /// </summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>
    /// Total amount of the sale (Grand Total including tax).
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// المجموع الفرعي قبل الضريبة
    /// Subtotal before VAT.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; } = 0;

    /// <summary>
    /// نسبة ضريبة القيمة المضافة المطبقة (%)
    /// VAT Rate applied (e.g. 15.00)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxRate { get; set; } = 0;

    /// <summary>
    /// إجمالي مبلغ ضريبة القيمة المضافة
    /// Total VAT amount for the invoice.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; } = 0;

    /// <summary>
    /// هل الأسعار شاملة الضريبة أم غير شاملة (مضافة)
    /// Indicates whether line prices are tax-inclusive (true) or tax-exclusive (false).
    /// </summary>
    public bool IsTaxInclusive { get; set; } = true;

    /// <summary>
    /// رمز الاستجابة السريعة المشفر وفق متطلبات هيئة الزكاة والضريبة والجمارك (ZATCA TLV Base64)
    /// </summary>
    [MaxLength(1000)]
    public string? ZatcaQrCode { get; set; }

    /// <summary>
    /// Total discount applied to the whole invoice.
    /// إجمالي الخصم المطبق على الفاتورة (يُسجل في حساب الخصومات المسموح بها).
    /// </summary>
    public decimal TotalDiscount { get; set; } = 0;

    /// <summary>
    /// Total cost of the medicines sold.
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Total profit from the sale.
    /// </summary>
    public decimal TotalProfit { get; set; }

    /// <summary>
    /// Payment method used.
    /// </summary>
    [Required]
    public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;

    /// <summary>
    /// Foreign key to the customer (optional for cash sales).
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Name of the customer (optional).
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Navigation property to the customer.
    /// </summary>
    [ForeignKey("CustomerId")]
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// هل تم تحصيل الفاتورة (للفواتير الآجلة أو المدفوعة جزئياً)
    /// Has the invoice been received (for credit or partially paid invoices)
    /// </summary>
    public bool IsPaid { get; set; } = false;

    /// <summary>
    /// المبلغ المدفوع من الفاتورة حتى الآن
    /// The total amount paid towards this invoice so far.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; } = 0;

    /// <summary>
    /// المبلغ المتبقي على الفاتورة
    /// The remaining unpaid balance on this invoice.
    /// </summary>
    [NotMapped]
    public decimal RemainingAmount => Math.Max(0, TotalAmount - PaidAmount);


    /// <summary>
    /// ID of the user who approved this invoice.
    /// </summary>
    public int? ApprovedBy { get; set; }

    /// <summary>
    /// Date and time when the invoice was approved.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// ID of the user who cancelled this invoice.
    /// </summary>
    public int? CancelledBy { get; set; }

    /// <summary>
    /// Date and time when the invoice was cancelled.
    /// </summary>
    public DateTime? CancelledAt { get; set; }


    /// <summary>
    /// Status of the document (Draft, Approved, Cancelled).
    /// </summary>
    public DocumentStatus Status { get; set; } = DocumentStatus.Approved;

    /// <summary>
    /// Collection of sale invoice details.
    /// </summary>
    public ICollection<SaleInvoiceDetail> SaleInvoiceDetails { get; set; }

    /// <summary>
    /// The ID of the cashier shift during which this invoice was created.
    /// </summary>
    public int? UserShiftId { get; set; }

    /// <summary>
    /// Navigation property to the user shift.
    /// </summary>
    [ForeignKey("UserShiftId")]
    public virtual UserShift? UserShift { get; set; }

    /// <summary>
    /// Collection of sales returns related to this invoice.
    /// </summary>
    public ICollection<SalesReturn> SalesReturns { get; set; }

    /// <summary>
    /// Collection of payments made for this invoice (Multi-payment).
    /// دفعات الفاتورة (للدفع المتعدد أو السداد)
    /// </summary>
    public virtual ICollection<SaleInvoicePayment> Payments { get; set; } = new List<SaleInvoicePayment>();

    // Multi-Branch Properties Inherited from BaseMultiBranchEntity

    // Navigation Properties

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User? Creator { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User? Approver { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User? Canceller { get; set; }
}