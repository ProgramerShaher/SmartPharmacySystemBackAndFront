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
            .Include(s => s.PurchaseInvoices.Where(i => !i.IsDeleted))
            .Include(s => s.PurchaseReturns.Where(r => !r.IsDeleted))
            .AsNoTracking()
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
    /// Optimized: AsNoTracking + StartsWith for indexed search + Data Projection
    /// </summary>
    public async Task<(IEnumerable<Supplier> Items, int TotalCount)> GetPagedAsync(
        string? search, int page, int pageSize, string sortBy, string sortDir, bool? hasBalance)
    {
        var query = _context.Suppliers
            .AsNoTracking() // 1. Access Optimization
            .Where(s => !s.IsDeleted);

        // 2. Optimized Search
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

        // 3. Sorting
        // 3. Sorting
        query = (sortBy?.ToLower(), sortDir?.ToLower()) switch
        {
            ("name", "desc") => query.OrderByDescending(s => s.Name),
            ("name", _) => query.OrderBy(s => s.Name),
            ("balance", "desc") => query.OrderByDescending(s => s.Balance),
            ("balance", _) => query.OrderBy(s => s.Balance),
            ("id", "desc") => query.OrderByDescending(s => s.Id),
            ("id", _) => query.OrderBy(s => s.Id),
            _ => query.OrderBy(s => s.Name)
        };

        // 4. Data Projection (Select only needed fields) & Pagination
        // Note: We return Supplier entity here to match Interface, but EF will only select these columns from SQL
        var items = await query
            .Select(s => new Supplier 
            {
                Id = s.Id,
                Name = s.Name,
                PhoneNumber = s.PhoneNumber,
                Balance = s.Balance,
                Address = s.Address,
                CreatedAt = s.CreatedAt,
                // Only fetch minimal required fields
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}