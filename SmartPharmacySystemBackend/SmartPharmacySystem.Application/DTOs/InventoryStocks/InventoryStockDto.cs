namespace SmartPharmacySystem.Application.DTOs.InventoryStocks;

/// <summary>
/// كائن نقل البيانات لعرض رصيد مخزون دواء في مخزن معين.
/// </summary>
public class InventoryStockDto
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string MedicineBarcode { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public string ExpiryStatus { get; set; } = string.Empty;
    public string ExpiryStatusColor { get; set; } = string.Empty;
    public int DaysUntilExpiry { get; set; }
    /// <summary>موقع الدفعة داخل المخزن (رف/ممر) — مثال: R1-S3</summary>
    public string? StorageLocation { get; set; }
}
