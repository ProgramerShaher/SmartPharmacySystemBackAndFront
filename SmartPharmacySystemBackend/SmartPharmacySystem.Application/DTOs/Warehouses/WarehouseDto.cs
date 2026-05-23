using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Warehouses;

/// <summary>
/// كائن نقل البيانات لعرض معلومات المخزن.
/// </summary>
public class WarehouseDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public WarehouseType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
