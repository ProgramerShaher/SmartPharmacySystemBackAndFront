using SmartPharmacySystem.Application.DTOs.Permission;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IPermissionService
{
    /// <summary>
    /// جلب جميع الصلاحيات مجمعة حسب الوحدة (Module)
    /// </summary>
    Task<IEnumerable<PermissionGroupDto>> GetAllPermissionsGroupedAsync();

    /// <summary>
    /// جلب جميع الصلاحيات الفعالة لمستخدم معين (مع الأخذ بالاعتبار دوره والاستثناءات)
    /// </summary>
    Task<List<string>> GetUserEffectivePermissionsAsync(int userId);

    /// <summary>
    /// جلب معرفات الصلاحيات الفعالة لمستخدم معين
    /// </summary>
    Task<List<int>> GetUserEffectivePermissionIdsAsync(int userId);

    /// <summary>
    /// تعيين الصلاحيات الفعالة لمستخدم معين وحفظ الاستثناءات
    /// </summary>
    Task AssignUserPermissionsAsync(int userId, List<int> selectedPermissionIds);
}
