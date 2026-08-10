namespace SmartPharmacySystem.Application.DTOs.Permission;

public class PermissionGroupDto
{
    public string Module { get; set; } = string.Empty;
    public string ModuleAr { get; set; } = string.Empty;
    public List<PermissionDto> Permissions { get; set; } = new();
}
