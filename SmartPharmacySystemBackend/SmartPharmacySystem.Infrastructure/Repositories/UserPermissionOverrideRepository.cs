using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// مستودع استثناءات الصلاحيات للمستخدمين
/// User permission override repository implementation
/// </summary>
public class UserPermissionOverrideRepository : IUserPermissionOverrideRepository
{
    private readonly ApplicationDbContext _context;

    public UserPermissionOverrideRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserPermissionOverride>> GetByUserIdAsync(int userId)
    {
        return await _context.UserPermissionOverrides
            .Include(o => o.Permission)
            .Where(o => o.UserId == userId && !o.IsDeleted)
            .ToListAsync();
    }

    public async Task<UserPermissionOverride?> GetByIdAsync(int id)
    {
        return await _context.UserPermissionOverrides
            .Include(o => o.Permission)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
    }

    public async Task<UserPermissionOverride?> GetByUserAndPermissionAsync(int userId, int permissionId)
    {
        return await _context.UserPermissionOverrides
            .FirstOrDefaultAsync(o => o.UserId == userId && o.PermissionId == permissionId && !o.IsDeleted);
    }

    public async Task AddAsync(UserPermissionOverride entity)
    {
        await _context.UserPermissionOverrides.AddAsync(entity);
    }

    public async Task UpdateAsync(UserPermissionOverride entity)
    {
        _context.UserPermissionOverrides.Update(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.UserPermissionOverrides.FindAsync(id);
        if (entity != null)
        {
            entity.SoftDelete();
        }
    }

    public async Task DeleteByUserAndPermissionAsync(int userId, int permissionId)
    {
        var entity = await GetByUserAndPermissionAsync(userId, permissionId);
        if (entity != null)
        {
            entity.SoftDelete();
        }
    }

    public async Task<IEnumerable<UserPermissionOverride>> GetActiveByUserIdAsync(int userId)
    {
        var now = DateTime.UtcNow;
        return await _context.UserPermissionOverrides
            .Include(o => o.Permission)
            .Where(o => o.UserId == userId
                     && !o.IsDeleted
                     && (o.ExpiresAt == null || o.ExpiresAt > now))
            .ToListAsync();
    }
}
