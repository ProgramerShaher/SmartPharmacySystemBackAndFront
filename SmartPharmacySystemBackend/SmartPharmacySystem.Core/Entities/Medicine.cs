namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents a medicine in the pharmacy management system.
/// Medicines are the core products managed in the system.
/// </summary>
public class Medicine
{
    /// <summary>
    /// Unique identifier for the medicine.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Internal code for the medicine used within the system.
    /// </summary>
    public string? InternalCode { get; set; }

    /// <summary>
    /// Name of the medicine.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Foreign key to the category this medicine belongs to.
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Manufacturer of the medicine.
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// Default barcode for the medicine.
    /// </summary>
    public string? DefaultBarcode { get; set; }

    /// <summary>
    /// Default purchase price per unit.
    /// </summary>
    public decimal DefaultPurchasePrice { get; set; }

    /// <summary>
    /// Default sale price per unit.
    /// </summary>
    public decimal DefaultSalePrice { get; set; }

    /// <summary>
    /// Minimum quantity that triggers an alert for low stock.
    /// </summary>
    public int MinAlertQuantity { get; set; }

    /// <summary>
    /// Indicates if the medicine is sold by unit (true) or by quantity (false).
    /// </summary>
    public bool SoldByUnit { get; set; }

    /// <summary>
    /// Status of the medicine (Active/Inactive).
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Additional notes about the medicine.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Date and time when the medicine was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the medicine was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Navigation property to the category.
    /// </summary>
    public Category Category { get; set; }

    /// <summary>
    /// Collection of medicine batches for this medicine.
    /// </summary>
    public ICollection<MedicineBatch> MedicineBatches { get; set; }

    /// <summary>
    /// Collection of inventory movements for this medicine.
    /// </summary>
    public ICollection<InventoryMovement> InventoryMovements { get; set; }
}