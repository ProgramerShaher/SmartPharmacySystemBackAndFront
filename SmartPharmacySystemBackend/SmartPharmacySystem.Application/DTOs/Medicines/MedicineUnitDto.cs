namespace SmartPharmacySystem.Application.DTOs.Medicine;

public class MedicineUnitDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ConversionFactor { get; set; }
    public decimal DefaultPurchasePrice { get; set; }
    public decimal DefaultSalePrice { get; set; }
    public string? Barcode { get; set; }
}
