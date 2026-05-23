namespace SmartPharmacySystem.Application.DTOs.InventoryStocks;

/// <summary>
/// كائن نقل البيانات للاستعلام عن أرصدة المخزون.
/// </summary>
public class InventoryStockQueryDto
{
    public int? WarehouseId { get; set; }
    public int? MedicineId { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryFrom { get; set; }
    public DateTime? ExpiryTo { get; set; }
    public bool? IsExpiringSoon { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
