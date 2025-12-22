namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents the details of a purchase invoice.
/// Each detail line corresponds to a specific medicine batch purchased.
/// </summary>
public class PurchaseInvoiceDetail
{
    /// <summary>
    /// Unique identifier for the purchase invoice detail.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the purchase invoice.
    /// </summary>
    public int PurchaseInvoiceId { get; set; }

    /// <summary>
    /// Foreign key to the medicine.
    /// </summary>
    public int MedicineId { get; set; }

    /// <summary>
    /// Foreign key to the medicine batch.
    /// </summary>
    public int BatchId { get; set; }

    /// <summary>
    /// Quantity purchased.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Purchase price per unit.
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// Total amount for this line (Quantity * PurchasePrice).
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Navigation property to the purchase invoice.
    /// </summary>
    public PurchaseInvoice PurchaseInvoice { get; set; }

    /// <summary>
    /// Navigation property to the medicine.
    /// </summary>
    public Medicine Medicine { get; set; }

    /// <summary>
    /// Navigation property to the medicine batch.
    /// </summary>
    public MedicineBatch Batch { get; set; }
}