namespace SmartPharmacySystem.Core.Entities;

public class AuditLog : BaseEntity
{

    public int? UserId { get; set; }
    public string? UserName { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? EntityType { get; set; }
    public int? EntityId { get; set; }

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }
    
    public string? Notes { get; set; }
}
