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
    /// اسم الدور بالعربية
    /// </summary>
    public string? NameAr { get; set; }

    /// <summary>
    /// وصف الدور
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// لون الدور
    /// </summary>
    public string Color { get; set; } = "#64748b";

    /// <summary>
    /// حالة الدور
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// معرفات الصلاحيات المرتبطة بالدور
    /// </summary>
    public List<int> PermissionIds { get; set; } = new();
}
