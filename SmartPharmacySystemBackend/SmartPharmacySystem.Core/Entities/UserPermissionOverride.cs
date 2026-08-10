using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Entities;

public class UserPermissionOverride : BaseEntity
{

    public int UserId { get; set; }
    public User? User { get; set; }

    public int PermissionId { get; set; }
    public Permission? Permission { get; set; }

    public GrantType GrantType { get; set; }
    public string? Reason { get; set; }
    
    public int? GrantedById { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
}
