using System.ComponentModel.DataAnnotations;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Branches;

/// <summary>
/// كائن نقل البيانات لعرض معلومات الفرع.
/// </summary>
public class BranchDto
{
    public int Id { get; set; }
    public string BranchCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public BranchType BranchType { get; set; }
    public string BranchTypeName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int WarehouseCount { get; set; }
    public int EmployeeCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
