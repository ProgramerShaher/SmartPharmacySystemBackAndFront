using SmartPharmacySystem.Application.DTOs.Shared;

namespace SmartPharmacySystem.Application.DTOs.StockMovement;

public class StockMovementQueryDto : BaseQueryDto
{
    public int? MedicineId { get; set; }
    public int? BatchId { get; set; }
    public string? MovementType { get; set; }
    public string? ReferenceType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? CreatedBy { get; set; }
}
