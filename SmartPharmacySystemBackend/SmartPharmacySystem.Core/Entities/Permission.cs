namespace SmartPharmacySystem.Core.Entities;

public class Permission : BaseEntity
{
    public string Module { get; set; } = string.Empty;
    public string ModuleAr { get; set; } = string.Empty;
    
    public string Action { get; set; } = string.Empty;
    public string ActionAr { get; set; } = string.Empty;
    
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserPermissionOverride> UserPermissionOverrides { get; set; } = new List<UserPermissionOverride>();
}
