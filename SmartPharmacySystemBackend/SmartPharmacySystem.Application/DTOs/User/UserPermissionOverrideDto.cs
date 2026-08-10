using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.User;

public class UserPermissionOverrideDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionNameAr { get; set; } = string.Empty;
    public GrantType GrantType { get; set; }
    public string? Reason { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive => !ExpiresAt.HasValue || ExpiresAt.Value > DateTime.UtcNow;
}

public class UpdateUserPermissionOverridesDto
{
    public List<UserPermissionOverrideRequestDto> Overrides { get; set; } = new();
}

public class UserPermissionOverrideRequestDto
{
    public int PermissionId { get; set; }
    public GrantType GrantType { get; set; }
    public string? Reason { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
