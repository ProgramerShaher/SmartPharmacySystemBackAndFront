using SmartPharmacySystem.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents sales returns in the pharmacy system.
/// Tracks returns of sold medicines.
/// </summary>
public class SalesReturn
{
    /// <summary>
    /// Unique identifier for the sales return.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the customer.
    /// </summary>
    public int? CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// Foreign key to the sale invoice.
    /// </summary>
    public int SaleInvoiceId { get; set; }

    /// <summary>
    /// Date of the return.
    /// </summary>
    public DateTime ReturnDate { get; set; }

    /// <summary>
    /// Total amount of the return.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// إجمالي التكلفة للأصناف المرتجعة
    /// Total cost of returned items
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// تأثير المرتجع على الربح (سالب)
    /// Profit impact of the return (negative)
    /// </summary>
    public decimal TotalProfit { get; set; }

    /// <summary>
    /// Reason for the return.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Date and time when the return was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// معرف المستخدم الذي أنشأ المرتجع
    /// ID of the user who created this return
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// ID of the user who approved this return.
    /// </summary>
    public int? ApprovedBy { get; set; }

    /// <summary>
    /// Date and time when the return was approved.
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// ID of the user who cancelled this return.
    /// </summary>
    public int? CancelledBy { get; set; }

    /// <summary>
    /// Date and time when the return was cancelled.
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
    /// Navigation property to the sale invoice.
    /// </summary>
    public SaleInvoice SaleInvoice { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User? Creator { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User? Approver { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User? Canceller { get; set; }

    /// <summary>
    /// Collection of sales return details.
    /// </summary>
    public ICollection<SalesReturnDetail> SalesReturnDetails { get; set; }
}