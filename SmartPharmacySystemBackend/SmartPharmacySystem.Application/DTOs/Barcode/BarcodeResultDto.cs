using SmartPharmacySystem.Application.DTOs.Medicine;

namespace SmartPharmacySystem.Application.DTOs.Barcode;

public class BarcodeResultDto
{
    public int MedicineId { get; set; }
    public string TradeName { get; set; } = string.Empty;
    public string? ScientificName { get; set; }
    public string? ActiveIngredient { get; set; }
    public string? StorageLocation { get; set; }
    public decimal MovingAverageCost { get; set; }

    // Nearest FIFO Batch Info
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public int AvailableQuantity { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsNearExpiry { get; set; }

    // Alternatives
    public List<MedicineAlternativeDto> Alternatives { get; set; } = new();
}

public class MedicineAlternativeDto
{
    public int MedicineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public int AvailableQuantity { get; set; }
}
