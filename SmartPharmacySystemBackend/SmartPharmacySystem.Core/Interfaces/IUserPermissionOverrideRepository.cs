using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// واجهة مستودع استثناءات الصلاحيات للمستخدمين
/// User permission override repository interface
/// </summary>
public interface IUserPermissionOverrideRepository
{
    Task<IEnumerable<UserPermissionOverride>> GetByUserIdAsync(int userId);
    Task<UserPermissionOverride?> GetByIdAsync(int id);
    Task<UserPermissionOverride?> GetByUserAndPermissionAsync(int userId, int permissionId);
    Task AddAsync(UserPermissionOverride entity);
    Task UpdateAsync(UserPermissionOverride entity);
    Task DeleteAsync(int id);
    Task DeleteByUserAndPermissionAsync(int userId, int permissionId);
    /// <summary>جلب الاستثناءات الفعّالة فقط (غير منتهية الصلاحية)</summary>
    Task<IEnumerable<UserPermissionOverride>> GetActiveByUserIdAsync(int userId);
}
