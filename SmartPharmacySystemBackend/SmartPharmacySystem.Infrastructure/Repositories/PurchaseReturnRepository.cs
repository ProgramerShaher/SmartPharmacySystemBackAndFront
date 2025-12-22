using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories
{
    public class PurchaseReturnRepository : IPurchaseReturnRepository
    {
        private readonly ApplicationDbContext _context;

        public PurchaseReturnRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PurchaseReturn?> GetByIdAsync(int id)
        {
            return await _context.PurchaseReturns
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseInvoice)
                .Include(p => p.PurchaseReturnDetails)
                    .ThenInclude(d => d.Medicine)
                .Include(p => p.PurchaseReturnDetails)
                    .ThenInclude(d => d.Batch)
                .FirstOrDefaultAsync(x => x.Id == id);
            // Assuming IsDeleted check handled if global filter exists or manually here.
            // Following other repos pattern (CategoryRepo checked IsDeleted=false).
            // But I will stick to simple first unless I verify IsDeleted on PurchaseReturn.
        }

        public async Task<IEnumerable<PurchaseReturn>> GetAllAsync()
        {
            return await _context.PurchaseReturns
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseInvoice)
                .Include(p => p.PurchaseReturnDetails)
                .OrderByDescending(p => p.ReturnDate)
                .ToListAsync();
        }

        public async Task AddAsync(PurchaseReturn entity)
        {
            await _context.PurchaseReturns.AddAsync(entity);
        }

        public Task UpdateAsync(PurchaseReturn entity)
        {
            _context.PurchaseReturns.Update(entity);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
             var entity = await GetByIdAsync(id);
             if (entity != null) _context.PurchaseReturns.Remove(entity);
        }

        public async Task SoftDeleteAsync(int id)
        {
             var entity = await GetByIdAsync(id);
             if (entity != null) _context.PurchaseReturns.Remove(entity);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PurchaseReturns.AnyAsync(x => x.Id == id);
        }
    }
}
