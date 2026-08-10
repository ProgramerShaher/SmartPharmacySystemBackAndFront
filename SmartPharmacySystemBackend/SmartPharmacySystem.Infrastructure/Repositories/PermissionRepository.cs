using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// مستودع الصلاحيات
/// Permission repository implementation
/// </summary>
public class PermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _context;

    public PermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        return await _context.Permissions
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Action)
            .ToListAsync();
    }

    public async Task<Permission?> GetByIdAsync(int id)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<Permission?> GetByCodeAsync(string code)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.Code == code && !p.IsDeleted);
    }

    public async Task<IEnumerable<Permission>> GetByModuleAsync(string module)
    {
        return await _context.Permissions
            .Where(p => p.Module == module && !p.IsDeleted)
            .OrderBy(p => p.Action)
            .ToListAsync();
    }

    public async Task AddAsync(Permission permission)
    {
        await _context.Permissions.AddAsync(permission);
    }

    public async Task UpdateAsync(Permission permission)
    {
        _context.Permissions.Update(permission);
    }

    public async Task<bool> ExistsAsync(string code)
    {
        return await _context.Permissions.AnyAsync(p => p.Code == code && !p.IsDeleted);
    }

    public async Task<IEnumerable<Permission>> FindAsync(System.Linq.Expressions.Expression<Func<Permission, bool>> predicate)
    {
        return await _context.Permissions.Where(predicate).ToListAsync();
    }
}
