using SmartPharmacySystem.Application.DTOs.Shared;

namespace SmartPharmacySystem.Application.DTOs.Medicine;

public class MedicineQueryDto : BaseQueryDto
{
    public int? CategoryId { get; set; }
    public string? Manufacturer { get; set; }
    public string? Status { get; set; } // Available, LowStock, OutOfStock
}
