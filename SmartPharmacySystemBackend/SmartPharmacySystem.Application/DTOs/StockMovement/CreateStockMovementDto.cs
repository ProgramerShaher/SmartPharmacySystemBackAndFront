using SmartPharmacySystem.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.StockMovement;

public class CreateStockMovementDto
{
    [Required]
    public int MedicineId { get; set; }

    public int? BatchId { get; set; }

    [Required]
    public StockMovementType MovementType { get; set; }

    [Required]
    public ReferenceType ReferenceType { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public int ReferenceId { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// For manual adjustments/damage (Internal use/Validation).
    /// </summary>
    public string? Reason { get; set; }
}
