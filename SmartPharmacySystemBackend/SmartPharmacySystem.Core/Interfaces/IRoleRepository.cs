using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// واجهة مستودع الأدوار
/// Role repository interface
/// </summary>
public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(int id);
    Task<Role?> GetByNameAsync(string name);
    Task AddAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(int id);

    /// <summary>جلب الدور مع صلاحياته كاملة</summary>
    Task<Role?> GetWithPermissionsAsync(int id);

    /// <summary>جلب كل الأدوار مع صلاحياتها</summary>
    Task<IEnumerable<Role>> GetAllWithPermissionsAsync();

    /// <summary>جلب كل صلاحيات IDs لدور معين</summary>
    Task<IEnumerable<int>> GetPermissionIdsAsync(int roleId);

    /// <summary>تحديث صلاحيات الدور (حذف القديمة وإضافة الجديدة)</summary>
    Task UpdatePermissionsAsync(int roleId, IEnumerable<int> permissionIds);

    /// <summary>نسخ دور بكل صلاحياته إلى دور جديد</summary>
    Task<Role> CloneAsync(int sourceRoleId, string newName, string? newNameAr, string? newColor);
}