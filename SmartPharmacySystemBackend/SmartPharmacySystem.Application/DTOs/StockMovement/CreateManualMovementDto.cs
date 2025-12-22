using SmartPharmacySystem.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartPharmacySystem.Application.DTOs.StockMovement;

public class CreateManualMovementDto
{
    [Required]
    public int MedicineId { get; set; }

    public int? BatchId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
    public int Quantity { get; set; } // Will be handled as negative internally for Damage, +/- for Adjustment

    [Required]
    public StockMovementType Type { get; set; } // Adjustment or Damage only

    [Required]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "يجب إدخال سبب منطقي للحركة (10 أحرف على الأقل)")]
    public string Reason { get; set; } = string.Empty;

    [Required]
    public int ApprovedBy { get; set; }
}
