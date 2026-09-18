namespace SmartPharmacySystem.Application.DTOs.StockCounts;

public class CreateStockCountItemDto
{
    public int MedicineId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public decimal SystemQuantity { get; set; }
    public decimal CountedQuantity { get; set; }
    public string? Notes { get; set; }
}
