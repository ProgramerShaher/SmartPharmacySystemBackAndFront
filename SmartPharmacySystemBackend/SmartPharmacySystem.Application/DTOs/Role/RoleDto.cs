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
    /// اسم الدور بالعربية
    /// </summary>
    public string? NameAr { get; set; }

    /// <summary>
    /// وصف الدور
    /// Role description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// لون الدور
    /// </summary>
    public string Color { get; set; } = "#64748b";

    /// <summary>
    /// هل الدور أساسي بالنظام
    /// </summary>
    public bool IsSystemRole { get; set; }

    /// <summary>
    /// حالة الدور
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// عدد المستخدمين في هذا الدور
    /// Number of users with this role
    /// </summary>
    public int UserCount { get; set; }

    /// <summary>
    /// عدد الصلاحيات الممنوحة
    /// Number of permissions
    /// </summary>
    public int PermissionCount { get; set; }

    /// <summary>
    /// تاريخ الإنشاء
    /// Creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
