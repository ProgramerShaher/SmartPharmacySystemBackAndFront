using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// Optimized supplier repository with AsNoTracking and efficient search
/// </summary>
public class SupplierRepository : ISupplierRepository
{
    private readonly ApplicationDbContext _context;

    public SupplierRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Supplier> GetByIdAsync(int id)
    {
        return await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<IEnumerable<Supplier>> GetAllAsync()
    {
        return await _context.Suppliers
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task AddAsync(Supplier entity)
    {
        await _context.Suppliers.AddAsync(entity);
    }

    public Task UpdateAsync(Supplier entity)
    {
        _context.Suppliers.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Suppliers.FindAsync(id);
        if (entity != null)
        {
            _context.Suppliers.Remove(entity);
        }
    }

    public async Task SoftDeleteAsync(int id)
    {
        var entity = await _context.Suppliers.FindAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            _context.Suppliers.Update(entity);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Suppliers
            .AsNoTracking()
            .AnyAsync(s => s.Id == id && !s.IsDeleted);
    }

    /// <summary>
    /// Optimized: AsNoTracking + StartsWith for indexed search
    /// </summary>
    public async Task<(IEnumerable<Supplier> Items, int TotalCount)> GetPagedAsync(
        string? search, int page, int pageSize, string sortBy, string sortDir, bool? hasBalance)
    {
        var query = _context.Suppliers
            .AsNoTracking()
            .Where(s => !s.IsDeleted);

        // ✅ Optimized: StartsWith for indexed search
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                s.Name.StartsWith(search) ||
                (s.PhoneNumber != null && s.PhoneNumber.StartsWith(search)) ||
                (s.Email != null && s.Email.StartsWith(search)));
        }

        // Apply balance filter
        if (hasBalance.HasValue)
        {
            query = hasBalance.Value
                ? query.Where(s => s.Balance > 0)
                : query.Where(s => s.Balance == 0);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // ✅ Explicit OrderBy with default
        query = string.IsNullOrWhiteSpace(sortBy) || sortBy == "Name"
            ? (sortDir?.ToLower() == "desc"
                ? query.OrderByDescending(s => s.Name)
                : query.OrderBy(s => s.Name))
            : (sortDir?.ToLower() == "desc"
                ? query.OrderByDescending(s => EF.Property<object>(s, sortBy))
                : query.OrderBy(s => EF.Property<object>(s, sortBy)));

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}