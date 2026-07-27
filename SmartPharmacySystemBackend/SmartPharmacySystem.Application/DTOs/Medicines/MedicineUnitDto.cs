namespace SmartPharmacySystem.Application.DTOs.Medicine;

public class MedicineUnitDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ConversionFactor { get; set; }
    public decimal DefaultPurchasePrice { get; set; }
    public decimal DefaultSalePrice { get; set; }
    public string? Barcode { get; set; }

    /// <summary>Whether this unit can be selected in a sales invoice.</summary>
    public bool IsAllowedForSale { get; set; } = true;

    /// <summary>Whether this unit can be selected in a purchase invoice.</summary>
    public bool IsAllowedForPurchase { get; set; } = true;

    /// <summary>Display order — higher = larger unit (Carton > Pack > Strip > Pill).</summary>
    public int SortOrder { get; set; }
}
