using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class BranchRepository : IBranchRepository
{
    private readonly ApplicationDbContext _context;

    public BranchRepository(ApplicationDbContext context) => _context = context;

    public async Task<Branch?> GetByIdAsync(int id)
    {
        return await _context.Branches
            .AsNoTracking()
            .Include(b => b.Warehouses)
            .Include(b => b.Employees)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
    }

    public async Task<Branch?> GetByCodeAsync(string branchCode)
    {
        return await _context.Branches
            .AsNoTracking()
            .Include(b => b.Warehouses)
            .FirstOrDefaultAsync(b => b.BranchCode == branchCode && !b.IsDeleted);
    }

    public async Task<IEnumerable<Branch>> GetAllAsync(string? search = null, bool? isActive = null, BranchType? type = null)
    {
        var query = _context.Branches.AsNoTracking().Where(b => !b.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => b.Name.Contains(search) || b.BranchCode.Contains(search) || (b.Location != null && b.Location.Contains(search)));

        if (isActive.HasValue)
            query = query.Where(b => b.IsActive == isActive.Value);

        if (type.HasValue)
            query = query.Where(b => b.BranchType == type.Value);

        return await query
            .Include(b => b.Warehouses)
            .OrderBy(b => b.BranchCode)
            .ToListAsync();
    }

    public async Task<Branch> AddAsync(Branch branch)
    {
        await _context.Branches.AddAsync(branch);
        return branch;
    }

    public async Task UpdateAsync(Branch branch)
    {
        _context.Branches.Update(branch);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch != null)
        {
            branch.IsDeleted = true;
            _context.Branches.Update(branch);
        }
    }

    public async Task<bool> CodeExistsAsync(string branchCode, int? excludeId = null)
    {
        var query = _context.Branches.AsNoTracking().Where(b => b.BranchCode == branchCode && !b.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(b => b.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task<IEnumerable<Branch>> GetActiveBranchesAsync()
    {
        return await _context.Branches
            .AsNoTracking()
            .Where(b => b.IsActive && !b.IsDeleted)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<int> GetBranchCountAsync()
    {
        return await _context.Branches.AsNoTracking().CountAsync(b => !b.IsDeleted);
    }

    public async Task<IEnumerable<Branch>> GetByTypeAsync(BranchType type)
    {
        return await _context.Branches
            .AsNoTracking()
            .Where(b => b.BranchType == type && !b.IsDeleted)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }
}
