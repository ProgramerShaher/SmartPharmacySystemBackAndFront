using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Warehouses;

public class UpdateWarehouseDto
{
    public int? BranchId { get; set; }
    public WarehouseType? Type { get; set; }
    public string? Name { get; set; }
}
