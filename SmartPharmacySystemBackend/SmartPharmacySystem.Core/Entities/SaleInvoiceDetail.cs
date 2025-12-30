namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents the details of a sale invoice.
/// Each detail line corresponds to a specific medicine batch sold.
/// </summary>
public class SaleInvoiceDetail
{
    /// <summary>
    /// Unique identifier for the sale invoice detail.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the sale invoice.
    /// </summary>
    public int SaleInvoiceId { get; set; }

    /// <summary>
    /// Foreign key to the medicine.
    /// </summary>
    public int MedicineId { get; set; }

    /// <summary>
    /// Foreign key to the medicine batch.
    /// </summary>
    public int BatchId { get; set; }

    /// <summary>
    /// Quantity sold.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Sale price per unit.
    /// </summary>
    public decimal SalePrice { get; set; }

    /// <summary>
    /// Unit cost per item.
    /// </summary>
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Total sale amount for this line (Quantity * SalePrice).
    /// </summary>
    public decimal TotalLineAmount { get; set; }

    /// <summary>
    /// Total cost for this line.
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Profit for this line.
    /// </summary>
    public decimal Profit { get; set; }

    /// <summary>
    /// Quantity remaining that can be returned.
    /// </summary>
    public int RemainingQtyToReturn { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Navigation property to the sale invoice.
    /// </summary>
    public SaleInvoice SaleInvoice { get; set; }

    /// <summary>
    /// Navigation property to the medicine.
    /// </summary>
    public Medicine Medicine { get; set; }

    /// <summary>
    /// Navigation property to the medicine batch.
    /// </summary>
    public MedicineBatch Batch { get; set; }
}