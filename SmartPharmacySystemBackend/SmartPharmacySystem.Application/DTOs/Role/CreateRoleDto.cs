namespace SmartPharmacySystem.Application.DTOs.Role;

/// <summary>
/// DTO لإنشاء دور جديد
/// Create role DTO
/// </summary>
public class CreateRoleDto
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
