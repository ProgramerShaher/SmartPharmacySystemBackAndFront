namespace SmartPharmacySystem.Application.DTOs.MedicineBatch;

/// <summary>
/// يمثل موقع تواجد الدفعة في المخازن المختلفة.
/// </summary>
public class BatchLocationDto
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
