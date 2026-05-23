namespace SmartPharmacySystem.Application.DTOs.MedicineWarehouseConfigs;

public class UpdateMedicineWarehouseConfigDto
{
    public int WarehouseId { get; set; }
    public int MedicineId { get; set; }
    public int ReorderLevel { get; set; }
    public int ReorderQuantity { get; set; }
}
