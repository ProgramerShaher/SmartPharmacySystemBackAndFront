using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.StockMovement;

public class StockMovementDto
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public int? BatchId { get; set; }
    public StockMovementType MovementType { get; set; }
    public ReferenceType ReferenceType { get; set; }
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
    public int ReferenceId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string FinancialDescription { get; set; } // «·Ê’› «·–Ì ”ÌÃ·» „‰ ÃœÊ· «·„«·Ì…
    public string? MedicineName { get; set; }
    public string? BatchNumber { get; set; }
}
