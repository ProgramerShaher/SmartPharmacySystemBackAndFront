using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories
{
    /// <summary>
    /// Optimized purchase invoice repository with AsNoTracking
    /// </summary>
    public class PurchaseInvoiceRepository : IPurchaseInvoiceRepository
    {
        private readonly ApplicationDbContext _context;

        public PurchaseInvoiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PurchaseInvoice?> GetByIdAsync(int id)
        {
            return await _context.PurchaseInvoices
                .AsNoTracking()
                .Include(i => i.Supplier)
                .Include(i => i.Warehouse)
                .Include(i => i.PurchaseInvoiceDetails)
                    .ThenInclude(d => d.Medicine)
                .Include(i => i.PurchaseInvoiceDetails)
                    .ThenInclude(d => d.Batch)
                .Include(i => i.Creator)
                .Include(i => i.Approver)
                .Include(i => i.Canceller)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task<IEnumerable<PurchaseInvoice>> GetAllAsync()
        {
            return await _context.PurchaseInvoices
                .AsNoTracking()
                .Include(i => i.Supplier)
                .Include(i => i.Warehouse)
                .Include(i => i.Creator)
                .Where(i => !i.IsDeleted)
                .OrderByDescending(i => i.CreatedAt)
                .Take(100)
                .ToListAsync();
        }

        public async Task AddAsync(PurchaseInvoice entity)
        {
            await _context.PurchaseInvoices.AddAsync(entity);
        }

        public Task UpdateAsync(PurchaseInvoice entity)
        {
            var trackedEntity = _context.PurchaseInvoices.Local.FirstOrDefault(e => e.Id == entity.Id);
            if (trackedEntity != null)
            {
                _context.Entry(trackedEntity).CurrentValues.SetValues(entity);
            }
            else
            {
                entity.Creator = null;
                entity.Approver = null;
                entity.Canceller = null;
                entity.Supplier = null;
                entity.Warehouse = null;

                if (entity.PurchaseInvoiceDetails != null)
                {
                    foreach (var detail in entity.PurchaseInvoiceDetails)
                    {
                        detail.Medicine = null;
                        detail.Batch = null;
                    }
                }

                var entry = _context.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    _context.PurchaseInvoices.Attach(entity);
                }
                entry.State = EntityState.Modified;
            }
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.PurchaseInvoices.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
            if (entity != null)
            {
                _context.PurchaseInvoices.Remove(entity);
            }
        }

        public async Task SoftDeleteAsync(int id)
        {
            var entity = await _context.PurchaseInvoices.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
            if (entity != null)
            {
                entity.IsDeleted = true;
                _context.PurchaseInvoices.Update(entity);
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PurchaseInvoices
                .AsNoTracking()
                .AnyAsync(e => e.Id == id && !e.IsDeleted);
        }

        public async Task<PurchaseInvoice?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.PurchaseInvoices
                .Include(i => i.PurchaseInvoiceDetails)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task<PurchaseInvoice?> GetByIdWithFullDetailsAsync(int id)
        {
            return await _context.PurchaseInvoices
                .AsNoTracking()
                .Include(i => i.Supplier)
                .Include(i => i.Warehouse)
                .Include(i => i.PurchaseInvoiceDetails)
                    .ThenInclude(d => d.Medicine)
                .Include(i => i.PurchaseInvoiceDetails)
                    .ThenInclude(d => d.Batch)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task<IEnumerable<PurchaseInvoice>> GetBySupplierIdAsync(int supplierId)
        {
            return await _context.PurchaseInvoices
                .AsNoTracking()
                .Where(i => i.SupplierId == supplierId && !i.IsDeleted && i.Status == Core.Enums.DocumentStatus.Approved)
                .OrderByDescending(i => i.PurchaseDate)
                .ToListAsync();
        }
    }
}
