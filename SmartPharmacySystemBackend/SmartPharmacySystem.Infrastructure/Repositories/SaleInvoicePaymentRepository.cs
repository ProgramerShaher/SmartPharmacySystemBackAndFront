using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories
{
    public class SaleInvoicePaymentRepository : ISaleInvoicePaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public SaleInvoicePaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SaleInvoicePayment?> GetByIdAsync(int id)
        {
            return await _context.SaleInvoicePayments
                .AsNoTracking()
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<IEnumerable<SaleInvoicePayment>> GetBySaleInvoiceIdAsync(int saleInvoiceId)
        {
            return await _context.SaleInvoicePayments
                .AsNoTracking()
                .Include(p => p.Account)
                .Where(p => p.SaleInvoiceId == saleInvoiceId && !p.IsDeleted)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(SaleInvoicePayment entity)
        {
            await _context.SaleInvoicePayments.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<SaleInvoicePayment> entities)
        {
            await _context.SaleInvoicePayments.AddRangeAsync(entities);
        }

        public async Task UpdateAsync(SaleInvoicePayment entity)
        {
            _context.SaleInvoicePayments.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.SaleInvoicePayments.FindAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                _context.SaleInvoicePayments.Update(entity);
            }
        }
    }
}
