using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories
{
    public class SupplierPaymentRepository : ISupplierPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public SupplierPaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SupplierPayment?> GetByIdAsync(int id)
        {
            // NOTE: `FindAsync` can bypass global query filters; use a filtered query instead.
            return await _context.SupplierPayments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<IEnumerable<SupplierPayment>> GetAllAsync()
        {
            return await _context.SupplierPayments
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task AddAsync(SupplierPayment entity)
        {
            await _context.SupplierPayments.AddAsync(entity);
        }

        public async Task UpdateAsync(SupplierPayment entity)
        {
            _context.SupplierPayments.Update(entity);
            // Wait for SaveChanges in UnitOfWork
            await Task.CompletedTask; 
        }

        public async Task<IEnumerable<SupplierPayment>> GetBySupplierIdAsync(int supplierId)
        {
            return await _context.SupplierPayments
                .Where(p => p.SupplierId == supplierId && !p.IsDeleted)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
    }
}
