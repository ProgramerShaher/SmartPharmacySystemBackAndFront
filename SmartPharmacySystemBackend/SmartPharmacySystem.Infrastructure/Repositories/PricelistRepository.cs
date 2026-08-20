using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class PricelistRepository : IPricelistRepository
{
    private readonly ApplicationDbContext _context;

    public PricelistRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Pricelist>> GetAllAsync(bool? isActive = null)
    {
        var query = _context.Pricelists
            .Include(p => p.Items)
                .ThenInclude(i => i.Medicine)
            .Where(p => !p.IsDeleted);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<Pricelist?> GetByIdAsync(int id)
    {
        return await _context.Pricelists
            .Where(p => p.Id == id && !p.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<Pricelist?> GetByIdWithItemsAsync(int id)
    {
        return await _context.Pricelists
            .Include(p => p.Items)
                .ThenInclude(i => i.Medicine)
            .Where(p => p.Id == id && !p.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<Pricelist> AddAsync(Pricelist pricelist)
    {
        await _context.Pricelists.AddAsync(pricelist);
        return pricelist;
    }

    public async Task UpdateAsync(Pricelist pricelist)
    {
        _context.Pricelists.Update(pricelist);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var pricelist = await GetByIdAsync(id);
        if (pricelist != null)
        {
            pricelist.IsDeleted = true;
            _context.Pricelists.Update(pricelist);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Pricelists.AnyAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<int> GetCustomersCountAsync(int pricelistId)
    {
        return await _context.Customers
            .CountAsync(c => c.PricelistId == pricelistId && !c.IsDeleted);
    }
}
