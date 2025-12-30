using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

/// <summary>
/// Represents inventory movements in the pharmacy system.
/// Tracks all changes to medicine stock levels.
/// </summary>
public class InventoryMovement
{
    /// <summary>
    /// Unique identifier for the inventory movement.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the medicine.
    /// </summary>
    public int MedicineId { get; private set; }

    /// <summary>
    /// Foreign key to the medicine batch (optional).
    /// </summary>
    public int? BatchId { get; private set; }

    /// <summary>
    /// Type of movement (Enum instead of string).
    /// </summary>
    public StockMovementType MovementType { get; private set; }

    /// <summary>
    /// Type of reference document.
    /// </summary>
    public ReferenceType ReferenceType { get; private set; }

    /// <summary>
    /// Quantity involved (Positive for Addition, Negative for Deduction).
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Date of the movement.
    /// </summary>
    public DateTime Date { get; private set; }

    /// <summary>
    /// Reference ID (Primary key of the reference document).
    /// </summary>
    public int ReferenceId { get; private set; }

    /// <summary>
    /// Reference Number (Display number of the reference document).
    /// </summary>
    public string ReferenceNumber { get; private set; } = string.Empty;

    /// <summary>
    /// ID of the user who performed the movement.
    /// </summary>
    public int CreatedBy { get; private set; }

    /// <summary>
    /// Additional notes or reason for manual adjustment.
    /// </summary>
    public string Notes { get;  set; } = string.Empty;

    public InventoryMovement() { } // For EF Core

    public InventoryMovement(int medicineId, int? batchId, StockMovementType movementType, ReferenceType referenceType, int quantity, int referenceId, string referenceNumber, int createdBy, string notes)
    {
        MedicineId = medicineId;
        BatchId = batchId;
        MovementType = movementType;
        ReferenceType = referenceType;
        Quantity = quantity;
        Date = DateTime.UtcNow;
        ReferenceId = referenceId;
        ReferenceNumber = referenceNumber;
        CreatedBy = createdBy;
        Notes = notes;
    }

    /// <summary>
    // Navigation Properties
    public virtual Medicine Medicine { get; private set; } = null!;
    public virtual MedicineBatch? Batch { get; private set; }
}