using SmartPharmacySystem.Application.DTOs.Role;

namespace SmartPharmacySystem.Application.Interfaces;

/// <summary>
/// واجهة خدمة الأدوار
/// Role service interface
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// الحصول على جميع الأدوار
    /// Get all roles
    /// </summary>
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();

    /// <summary>
    /// الحصول على دور بالمعرف
    /// Get role by ID
    /// </summary>
    Task<RoleDto?> GetRoleByIdAsync(int id);

    /// <summary>
    /// الحصول على دور بالاسم
    /// Get role by name
    /// </summary>
    Task<RoleDto?> GetRoleByNameAsync(string name);

    /// <summary>
    /// إنشاء دور جديد
    /// Create new role
    /// </summary>
    Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);

    /// <summary>
    /// تحديث دور
    /// Update role
    /// </summary>
    Task UpdateRoleAsync(int id, UpdateRoleDto dto);

    /// <summary>
    /// حذف دور
    /// Delete role
    /// </summary>
    Task DeleteRoleAsync(int id);
}
