namespace SmartPharmacySystem.Application.DTOs.InventoryStocks;

public class UpdateInventoryStockDto
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public int MedicineId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int Quantity { get; set; }
    /// <summary>موقع التخزين داخل المخزن — اختياري — مثال: "R1-S3"</summary>
    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string? StorageLocation { get; set; }
}
