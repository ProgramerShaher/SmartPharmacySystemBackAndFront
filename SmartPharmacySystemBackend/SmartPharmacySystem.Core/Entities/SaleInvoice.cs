using SmartPharmacySystem.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a sale invoice in the pharmacy system.
/// Sale invoices record medicine sales to customers.
/// </summary>
public class SaleInvoice
{
    /// <summary>
    /// Unique identifier for the sale invoice.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique number for the sale invoice (Format: SI-YYYY-######).
    /// </summary>
    public string SaleInvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Date of the sale.
    /// </summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>
    /// Total amount of the sale.
    /// </summary>
    public decimal TotalAmount { get; set; }

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
    /// هل تم تحصيل الفاتورة (للفواتير الآجلة)
    /// Has the invoice been received (for credit invoices)
    /// </summary>
    public bool IsPaid { get; set; } = false;

    /// <summary>
    /// Date and time when the invoice was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// ID of the user who created this invoice.
    /// معرف المستخدم الذي أنشأ هذه الفاتورة.
    /// </summary>
    public int CreatedBy { get; set; }

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
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Status of the document (Draft, Approved, Cancelled).
    /// </summary>
    public DocumentStatus Status { get; set; } = DocumentStatus.Approved;

    /// <summary>
    /// Collection of sale invoice details.
    /// </summary>
    public ICollection<SaleInvoiceDetail> SaleInvoiceDetails { get; set; }

    /// <summary>
    /// Collection of sales returns related to this invoice.
    /// </summary>
    public ICollection<SalesReturn> SalesReturns { get; set; }

    // Navigation Properties

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User? Creator { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User? Approver { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User? Canceller { get; set; }
}