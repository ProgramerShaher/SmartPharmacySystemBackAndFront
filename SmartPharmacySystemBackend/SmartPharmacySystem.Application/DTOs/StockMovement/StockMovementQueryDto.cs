using SmartPharmacySystem.Application.DTOs.Shared;

namespace SmartPharmacySystem.Application.DTOs.StockMovement;

public class StockMovementQueryDto : BaseQueryDto
{
    public int? MedicineId { get; set; }
    public string? MovementType { get; set; }
}
