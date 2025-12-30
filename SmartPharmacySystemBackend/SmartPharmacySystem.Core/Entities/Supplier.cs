namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a supplier in the pharmacy management system.
/// Suppliers provide medicines through purchase invoices.
/// </summary>
public class Supplier
{
    /// <summary>
    /// Unique identifier for the supplier.
    /// </summary>
    /// <summary>
    /// Unique identifier for the supplier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the supplier.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Phone number for contact.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Address of the supplier.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Email address for communication.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Current balance owed to the supplier.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Additional notes about the supplier.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Date and time when the supplier was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the supplier was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Collection of purchase invoices from this supplier.
    /// </summary>
    public ICollection<PurchaseInvoice> PurchaseInvoices { get; set; }

    /// <summary>
    /// Collection of purchase returns to this supplier.
    /// </summary>
    public ICollection<PurchaseReturn> PurchaseReturns { get; set; }
}