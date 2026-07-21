using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// Implements the sales return repository for data access operations.
/// This class provides concrete implementations of sales return data operations.
/// </summary>
public class SalesReturnRepository : ISalesReturnRepository
{
    private readonly ApplicationDbContext _context;

    public SalesReturnRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalesReturn> GetByIdAsync(int id)
    {
        return await _context.SalesReturns
            .Include(r => r.SaleInvoice)
            .Include(r => r.SalesReturnDetails)
                .ThenInclude(d => d.Medicine)
            .Include(r => r.SalesReturnDetails)
                .ThenInclude(d => d.Batch)
            .Include(r => r.Creator)
            .Include(r => r.Approver)
            .Include(r => r.Canceller)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }

    public async Task<IEnumerable<SalesReturn>> GetAllAsync()
    {
        return await _context.SalesReturns
            .AsNoTracking()
            .Include(r => r.SaleInvoice)
            .Include(r => r.SalesReturnDetails)
            .Include(r => r.Creator)
            .Include(r => r.Approver)
            .Include(r => r.Canceller)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.ReturnDate)
            .ToListAsync();
    }

    public async Task AddAsync(SalesReturn entity)
    {
        await _context.SalesReturns.AddAsync(entity);
    }

    public Task UpdateAsync(SalesReturn entity)
    {
        _context.SalesReturns.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        // NOTE: `FindAsync` can bypass global query filters; use a filtered query instead.
        var entity = await _context.SalesReturns.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (entity != null)
        {
            _context.SalesReturns.Remove(entity);
        }
    }

    public async Task SoftDeleteAsync(int id)
    {
        // NOTE: `FindAsync` can bypass global query filters; use a filtered query instead.
        var entity = await _context.SalesReturns.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (entity != null)
        {
            entity.IsDeleted = true;
            _context.SalesReturns.Update(entity);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.SalesReturns.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDeleted);
    }

    public async Task<IEnumerable<SalesReturn>> GetBySaleInvoiceIdAsync(int saleInvoiceId)
    {
        return await _context.SalesReturns
            .AsNoTracking()
            .Include(r => r.SalesReturnDetails)
            .Where(r => r.SaleInvoiceId == saleInvoiceId && !r.IsDeleted)
            .ToListAsync();
    }
}
