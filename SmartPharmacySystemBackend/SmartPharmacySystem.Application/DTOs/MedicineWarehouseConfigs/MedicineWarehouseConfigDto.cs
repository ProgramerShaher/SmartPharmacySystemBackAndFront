namespace SmartPharmacySystem.Application.DTOs.MedicineWarehouseConfigs;

/// <summary>
/// كائن نقل البيانات لعرض إعدادات دواء في مخزن معين.
/// </summary>
public class MedicineWarehouseConfigDto
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int ReorderLevel { get; set; }
    public int ReorderQuantity { get; set; }
    public int CurrentStock { get; set; }
    public bool IsBelowReorderLevel { get; set; }
}
