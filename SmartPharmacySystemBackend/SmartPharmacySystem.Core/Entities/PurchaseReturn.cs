using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents purchase returns in the pharmacy system.
/// Tracks returns of purchased medicines to suppliers.
/// </summary>
public class PurchaseReturn
{
    /// <summary>
    /// Unique identifier for the purchase return.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the purchase invoice.
    /// </summary>
    public int PurchaseInvoiceId { get; set; }

    /// <summary>
    /// Foreign key to the supplier.
    /// </summary>
    public int SupplierId { get; set; }

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
    /// ID of the user who created this return.
    /// معرف المستخدم الذي أنشأ هذا المرتجع.
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Status of the document (Draft, Approved, Cancelled).
    /// </summary>
    public DocumentStatus Status { get; set; } = DocumentStatus.Approved;

    /// <summary>
    /// Navigation property to the purchase invoice.
    /// </summary>
    public PurchaseInvoice PurchaseInvoice { get; set; }

    /// <summary>
    /// Navigation property to the supplier.
    /// </summary>
    public Supplier Supplier { get; set; }

    /// <summary>
    /// Collection of purchase return details.
    /// </summary>
    public ICollection<PurchaseReturnDetail> PurchaseReturnDetails { get; set; }
}