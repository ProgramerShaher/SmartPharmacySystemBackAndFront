namespace SmartPharmacySystem.Application.DTOs.Role;

/// <summary>
/// DTO لتحديث دور
/// Update role DTO
/// </summary>
public class UpdateRoleDto
{
    /// <summary>
    /// اسم الدور
    /// Role name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// وصف الدور
    /// Role description
    /// </summary>
    public string? Description { get; set; }
}
