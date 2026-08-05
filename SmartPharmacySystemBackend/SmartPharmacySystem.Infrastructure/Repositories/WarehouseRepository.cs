using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly ApplicationDbContext _context;

    public WarehouseRepository(ApplicationDbContext context) => _context = context;

    public async Task<Warehouse?> GetByIdAsync(int id)
    {
        return await _context.Warehouses
            .AsNoTracking()
            .Include(w => w.Branch)
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync(int? branchId = null, WarehouseType? type = null)
    {
        var query = _context.Warehouses.AsNoTracking().Where(w => !w.IsDeleted).AsQueryable();

        // Auto-filter by current branch if not explicitly specified
        branchId ??= _context.CurrentBranchId;

        if (branchId.HasValue)
            query = query.Where(w => w.BranchId == branchId.Value);

        if (type.HasValue)
            query = query.Where(w => w.Type == type.Value);

        return await query
            .Include(w => w.Branch)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Warehouse>> GetByBranchIdAsync(int branchId)
    {
        return await _context.Warehouses
            .AsNoTracking()
            .Include(w => w.Branch)
            .Where(w => w.BranchId == branchId && !w.IsDeleted)
            .OrderBy(w => w.Type)
            .ToListAsync();
    }

    public async Task<Warehouse?> GetByBranchAndTypeAsync(int branchId, WarehouseType type)
    {
        return await _context.Warehouses
            .AsNoTracking()
            .Include(w => w.Branch)
            .FirstOrDefaultAsync(w => w.BranchId == branchId && w.Type == type && !w.IsDeleted);
    }

    public async Task<Warehouse> AddAsync(Warehouse warehouse)
    {
        await _context.Warehouses.AddAsync(warehouse);
        return warehouse;
    }

    public async Task UpdateAsync(Warehouse warehouse)
    {
        _context.Warehouses.Update(warehouse);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        // NOTE: `FindAsync` can bypass global query filters; use a filtered query instead.
        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
        if (warehouse != null)
        {
            warehouse.IsDeleted = true;
            _context.Warehouses.Update(warehouse);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Warehouses.AsNoTracking().AnyAsync(w => w.Id == id && !w.IsDeleted);
    }

    public async Task<int> GetWarehouseCountAsync(int branchId)
    {
        return await _context.Warehouses.AsNoTracking().CountAsync(w => w.BranchId == branchId && !w.IsDeleted);
    }
}
