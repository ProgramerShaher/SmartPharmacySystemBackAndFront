using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Branches;

public class UpdateBranchDto
{
    public int Id { get; set; }
    public string BranchCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public BranchType BranchType { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
}