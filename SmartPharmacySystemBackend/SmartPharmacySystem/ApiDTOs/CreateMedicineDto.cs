namespace SmartPharmacySystem.ApiDTOs;

/// <summary>
/// كائن نقل البيانات لإنشاء دواء جديد.
/// </summary>
public class CreateMedicineDto
{
    public string InternalCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string DefaultBarcode { get; set; } = string.Empty;
    public decimal DefaultPurchasePrice { get; set; }
    public decimal DefaultSalePrice { get; set; }
    public int MinAlertQuantity { get; set; }
    public bool SoldByUnit { get; set; }
    public string Notes { get; set; } = string.Empty;
}
