namespace SmartPharmacySystem.Application.DTOs.Permission;

public class PermissionDto
{
    public int Id { get; set; }
    public string Module { get; set; } = string.Empty;
    public string ModuleAr { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ActionAr { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
}
