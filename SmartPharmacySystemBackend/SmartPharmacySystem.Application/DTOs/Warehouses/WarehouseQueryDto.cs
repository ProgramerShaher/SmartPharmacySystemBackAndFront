using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Warehouses;

/// <summary>
/// كائن نقل البيانات للاستعلام عن المخازن.
/// </summary>
public class WarehouseQueryDto
{
    public int? BranchId { get; set; }
    public WarehouseType? Type { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
