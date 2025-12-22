using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories
{
    public class SaleInvoiceDetailRepository : ISaleInvoiceDetailRepository
    {
        private readonly ApplicationDbContext _context;

        public SaleInvoiceDetailRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SaleInvoiceDetail?> GetByIdAsync(int id)
        {
            return await _context.SaleInvoiceDetails
                .Include(d => d.Medicine)
                .Include(d => d.Batch)
                .Include(d => d.SaleInvoice)
                    .ThenInclude(i => i.SaleInvoiceDetails)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<SaleInvoiceDetail>> GetAllAsync()
        {
            return await _context.SaleInvoiceDetails
                .Include(d => d.Medicine)
                .Include(d => d.Batch)
                .Include(d => d.SaleInvoice)
                    .ThenInclude(i => i.SaleInvoiceDetails)
                .ToListAsync();
        }

        public async Task AddAsync(SaleInvoiceDetail entity)
        {
            await _context.SaleInvoiceDetails.AddAsync(entity);
        }

        public Task UpdateAsync(SaleInvoiceDetail entity)
        {
            _context.SaleInvoiceDetails.Update(entity);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null) _context.SaleInvoiceDetails.Remove(entity);
        }

        public async Task SoftDeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null) _context.SaleInvoiceDetails.Remove(entity);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.SaleInvoiceDetails.AnyAsync(x => x.Id == id);
        }
    }
}
