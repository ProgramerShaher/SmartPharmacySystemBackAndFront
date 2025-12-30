namespace SmartPharmacySystem.Application.DTOs.Role;

/// <summary>
/// DTO لعرض معلومات الدور
/// Role display DTO
/// </summary>
public class RoleDto
{
    /// <summary>
    /// معرف الدور
    /// Role ID
    /// </summary>
    public int Id { get; set; }

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

    /// <summary>
    /// عدد المستخدمين في هذا الدور
    /// Number of users with this role
    /// </summary>
    public int UserCount { get; set; }

    /// <summary>
    /// تاريخ الإنشاء
    /// Creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
