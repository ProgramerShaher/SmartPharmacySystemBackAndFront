using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// مستودع الأدوار — محدّث لدعم نظام RBAC
/// Role repository implementation with RBAC support
/// </summary>
public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Roles
            .Include(r => r.Users)
            .Where(r => !r.IsDeleted)
            .ToListAsync();
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        return await _context.Roles
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == name && !r.IsDeleted);
    }

    public async Task AddAsync(Role role)
    {
        await _context.Roles.AddAsync(role);
    }

    public async Task UpdateAsync(Role role)
    {
        _context.Roles.Update(role);
    }

    public async Task DeleteAsync(int id)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (role != null)
        {
            // حماية: لا يمكن حذف أدوار النظام
            if (role.IsSystemRole)
                throw new InvalidOperationException($"لا يمكن حذف الدور '{role.Name}' لأنه دور نظام أساسي.");

            role.SoftDelete();
        }
    }

    public async Task<Role?> GetWithPermissionsAsync(int id)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }

    public async Task<IEnumerable<Role>> GetAllWithPermissionsAsync()
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.Users)
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<int>> GetPermissionIdsAsync(int roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
            .Select(rp => rp.PermissionId)
            .ToListAsync();
    }

    public async Task UpdatePermissionsAsync(int roleId, IEnumerable<int> permissionIds)
    {
        var existingPermissions = await _context.RolePermissions
            .IgnoreQueryFilters() // Just in case a global filter is added later
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        var selectedIds = permissionIds.Distinct().ToList();

        // 1. Revive or keep existing
        foreach (var selectedId in selectedIds)
        {
            var existing = existingPermissions.FirstOrDefault(rp => rp.PermissionId == selectedId);
            if (existing != null)
            {
                if (existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                    existing.DeletedAt = null;
                    _context.Entry(existing).State = EntityState.Modified;
                }
            }
            else
            {
                // Add new
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = selectedId,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }
        }

        // 2. Remove (soft-delete) unselected
        var toRemove = existingPermissions.Where(rp => !selectedIds.Contains(rp.PermissionId) && !rp.IsDeleted).ToList();
        foreach (var rp in toRemove)
        {
            rp.IsDeleted = true;
            rp.DeletedAt = DateTime.UtcNow;
            _context.Entry(rp).State = EntityState.Modified;
        }
    }

    public async Task<Role> CloneAsync(int sourceRoleId, string newName, string? newNameAr, string? newColor)
    {
        var source = await GetWithPermissionsAsync(sourceRoleId)
            ?? throw new InvalidOperationException($"الدور المصدر بالمعرف {sourceRoleId} غير موجود.");

        var cloned = new Role
        {
            Name = newName,
            NameAr = newNameAr ?? newName,
            Description = $"نسخة من: {source.NameAr ?? source.Name}",
            Color = newColor ?? source.Color,
            IsSystemRole = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Roles.AddAsync(cloned);
        await _context.SaveChangesAsync();

        // نسخ الصلاحيات
        var permIds = source.RolePermissions
            .Where(rp => !rp.IsDeleted)
            .Select(rp => rp.PermissionId)
            .ToList();

        await UpdatePermissionsAsync(cloned.Id, permIds);

        return cloned;
    }
}
