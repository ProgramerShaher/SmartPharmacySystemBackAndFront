namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents the details of a sales return.
/// Each detail line corresponds to a specific medicine batch returned.
/// </summary>
public class SalesReturnDetail
{
    /// <summary>
    /// Unique identifier for the sales return detail.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the sales return.
    /// </summary>
    public int SalesReturnId { get; set; }

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
    /// Sale price per unit at the time of return.
    /// </summary>
    public decimal SalePrice { get; set; }

    /// <summary>
    /// Cost per unit at the time of return.
    /// </summary>
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Total return amount for this line.
    /// </summary>
    public decimal TotalReturn { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Navigation property to the sales return.
    /// </summary>
    public SalesReturn SalesReturn { get; set; }

    /// <summary>
    /// Navigation property to the medicine.
    /// </summary>
    public Medicine Medicine { get; set; }

    /// <summary>
    /// Navigation property to the medicine batch.
    /// </summary>
    public MedicineBatch Batch { get; set; }
}