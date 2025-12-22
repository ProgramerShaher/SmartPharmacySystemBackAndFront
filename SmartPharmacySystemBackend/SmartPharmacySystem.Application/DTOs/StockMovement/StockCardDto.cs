using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.StockMovement;

public class StockCardDto
{
    public DateTime Date { get; set; }
    public StockMovementType MovementType { get; set; }
    public int QuantityChange { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public int RunningBalance { get; set; }
    public string? Notes { get; set; }
}
