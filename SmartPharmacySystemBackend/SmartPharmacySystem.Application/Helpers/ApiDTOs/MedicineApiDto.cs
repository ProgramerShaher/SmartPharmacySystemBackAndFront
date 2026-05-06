namespace SmartPharmacySystem.ApiDTOs;

/// <summary>
/// كائن نقل البيانات للدواء في API.
/// يُستخدم هذا الكلاس لتسلسل الطلبات والاستجابات في API.
/// </summary>
public class MedicineApiDto
{
    public int Id { get; set; }
    public string InternalCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string DefaultBarcode { get; set; } = string.Empty;
    public decimal DefaultPurchasePrice { get; set; }
    public decimal DefaultSalePrice { get; set; }
    public int MinAlertQuantity { get; set; }
    public bool SoldByUnit { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}