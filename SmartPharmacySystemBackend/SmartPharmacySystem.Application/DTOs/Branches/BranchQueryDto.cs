using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Branches;

/// <summary>
/// كائن نقل البيانات للاستعلام عن الفروع.
/// </summary>
public class BranchQueryDto
{
    public string? Search { get; set; }
    public BranchType? BranchType { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
