namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents the details of a purchase return.
/// Each detail line corresponds to a specific medicine batch returned.
/// </summary>
public class PurchaseReturnDetail
{
    /// <summary>
    /// Unique identifier for the purchase return detail.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the purchase return.
    /// </summary>
    public int PurchaseReturnId { get; set; }

    /// <summary>
    /// Foreign key to the medicine.
    /// </summary>
    public int MedicineId { get; set; }

    /// <summary>
    /// Foreign key to the medicine batch.
    /// </summary>
    public int BatchId { get; set; }

    /// <summary>
    /// Quantity returned.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Purchase price per unit at the time of return.
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// Total return amount for this line.
    /// </summary>
    public decimal TotalReturn { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Navigation property to the purchase return.
    /// </summary>
    public PurchaseReturn PurchaseReturn { get; set; }

    /// <summary>
    /// Navigation property to the medicine.
    /// </summary>
    public Medicine Medicine { get; set; }

    /// <summary>
    /// Navigation property to the medicine batch.
    /// </summary>
    public MedicineBatch Batch { get; set; }
}