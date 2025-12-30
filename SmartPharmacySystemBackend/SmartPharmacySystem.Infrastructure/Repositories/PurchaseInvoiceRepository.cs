using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Infrastructure.Repositories
{
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
                .Include(i => i.Supplier) // Include Supplier
                .Include(i => i.PurchaseInvoiceDetails)
                    .ThenInclude(d => d.Medicine)
                .Include(i => i.PurchaseInvoiceDetails)
                    .ThenInclude(d => d.Batch)
                .Include(i => i.Creator)
                .Include(i => i.Approver)
                .Include(i => i.Canceller)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<PurchaseInvoice>> GetAllAsync()
        {
            return await _context.PurchaseInvoices
                .Include(i => i.Supplier) // Include Supplier
                .Include(i => i.PurchaseInvoiceDetails)
                    .ThenInclude(d => d.Medicine)
                 .Include(i => i.PurchaseInvoiceDetails)
                    .ThenInclude(d => d.Batch)
                .Include(i => i.Creator)
                .Include(i => i.Approver)
                .Include(i => i.Canceller)
                .OrderByDescending(i => i.CreatedAt)
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
            return await _context.PurchaseInvoices.AnyAsync(e => e.Id == id);
        }

        public async Task<PurchaseInvoice?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.PurchaseInvoices
                .Include(i => i.PurchaseInvoiceDetails)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
    }
}
