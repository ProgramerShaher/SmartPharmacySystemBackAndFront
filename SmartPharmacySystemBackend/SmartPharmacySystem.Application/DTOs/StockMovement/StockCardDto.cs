using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.StockMovement;

public class StockCardDto
{
    public DateTime Date { get; set; }
    public StockMovementType MovementType { get; set; }
    public decimal QuantityChange { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal RunningBalance { get; set; }
    public string? Notes { get; set; }
}
