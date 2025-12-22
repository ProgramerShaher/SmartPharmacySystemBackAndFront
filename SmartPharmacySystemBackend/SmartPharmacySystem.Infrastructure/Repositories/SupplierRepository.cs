using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// Implements the supplier repository for data access operations.
/// This class provides concrete implementations of supplier data operations.
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
        return await _context.Suppliers.FindAsync(id);
    }

    public async Task<IEnumerable<Supplier>> GetAllAsync()
    {
        return await _context.Suppliers.ToListAsync();
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
        return await _context.Suppliers.AnyAsync(s => s.Id == id);
    }

    public async Task<(IEnumerable<Supplier> Items, int TotalCount)> GetPagedAsync(
        string? search, int page, int pageSize, string sortBy, string sortDir, bool? hasBalance)
    {
        var query = _context.Suppliers.Where(s => !s.IsDeleted);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(search) ||
                                     (s.PhoneNumber != null && s.PhoneNumber.Contains(search)) ||
                                     (s.Email != null && s.Email.ToLower().Contains(search)));
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

        // Apply sorting
        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(s => EF.Property<object>(s, sortBy))
            : query.OrderBy(s => EF.Property<object>(s, sortBy));

        // Apply pagination
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }
}