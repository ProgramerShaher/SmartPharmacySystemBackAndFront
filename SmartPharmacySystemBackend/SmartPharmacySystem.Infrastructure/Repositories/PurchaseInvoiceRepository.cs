using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

/// <summary>
/// Implements the purchase invoice repository for data access operations.
/// This class provides concrete implementations of purchase invoice data operations.
/// </summary>
public class PurchaseInvoiceRepository : IPurchaseInvoiceRepository
{
    private readonly ApplicationDbContext _context;

    public PurchaseInvoiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseInvoice> GetByIdAsync(int id)
    {
        return await _context.PurchaseInvoices
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseInvoiceDetails)
                .ThenInclude(pid => pid.Medicine)
            .Include(p => p.PurchaseInvoiceDetails)
                .ThenInclude(pid => pid.Batch)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<PurchaseInvoice>> GetAllAsync()
    {
        return await _context.PurchaseInvoices
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseInvoiceDetails)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();
    }

    public async Task AddAsync(PurchaseInvoice entity)
    {
        await _context.PurchaseInvoices.AddAsync(entity);
    }

    public Task UpdateAsync(PurchaseInvoice entity)
    {
        _context.PurchaseInvoices.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.PurchaseInvoices.FindAsync(id);
        if (entity != null)
        {
            _context.PurchaseInvoices.Remove(entity);
        }
    }

    public async Task SoftDeleteAsync(int id)
    {
        var entity = await _context.PurchaseInvoices.FindAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            _context.PurchaseInvoices.Update(entity);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.PurchaseInvoices.AnyAsync(pi => pi.Id == id);
    }
}