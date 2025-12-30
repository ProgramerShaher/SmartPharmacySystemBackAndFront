using SmartPharmacySystem.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a purchase invoice in the pharmacy system.
/// Purchase invoices record medicine purchases from suppliers.
/// </summary>
public class PurchaseInvoice
{
    /// <summary>
    /// Unique identifier for the purchase invoice.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique number for the purchase invoice (Format: PI-YYYY-######).
    /// </summary>
    public string PurchaseInvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to the supplier.
    /// </summary>
    public int SupplierId { get; set; }

    /// <summary>
    /// Invoice number provided by the supplier.
    /// </summary>
    public string? SupplierInvoiceNumber { get; set; }

    /// <summary>
    /// Date of the purchase.
    /// </summary>
    public DateTime PurchaseDate { get; set; }

    /// <summary>
    /// Total amount of the invoice.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Payment method used (Cash/Credit).
    /// </summary>
    [Required]
    public PaymentType PaymentMethod { get; set; } = PaymentType.Cash;

    /// <summary>
    /// Additional notes about the invoice.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// هل تم سداد الفاتورة (للفواتير الآجلة)
    /// Has the invoice been paid (for credit invoices)
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
    /// Navigation property to the supplier.
    /// </summary>
    public Supplier Supplier { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User? Creator { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User? Approver { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User? Canceller { get; set; }

    /// <summary>
    /// Collection of purchase invoice details.
    /// </summary>
    public ICollection<PurchaseInvoiceDetail> PurchaseInvoiceDetails { get; set; }
}