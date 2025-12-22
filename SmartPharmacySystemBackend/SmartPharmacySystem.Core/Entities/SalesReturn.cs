using SmartPharmacySystem.Core.Enums;

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
    /// Reason for the return.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Date and time when the return was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

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

    /// <summary>
    /// Collection of sales return details.
    /// </summary>
    public ICollection<SalesReturnDetail> SalesReturnDetails { get; set; }
}