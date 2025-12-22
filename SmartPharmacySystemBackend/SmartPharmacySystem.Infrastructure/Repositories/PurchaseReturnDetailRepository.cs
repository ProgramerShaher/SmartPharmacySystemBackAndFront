using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories
{
    public class PurchaseReturnDetailRepository : IPurchaseReturnDetailRepository
    {
        private readonly ApplicationDbContext _context;

        public PurchaseReturnDetailRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PurchaseReturnDetail?> GetByIdAsync(int id)
        {
            return await _context.PurchaseReturnDetails
                .Include(d => d.Medicine)
                .Include(d => d.Batch)
                .Include(d => d.PurchaseReturn)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<PurchaseReturnDetail>> GetAllAsync()
        {
            return await _context.PurchaseReturnDetails
                .Include(d => d.Medicine)
                .Include(d => d.Batch)
                .Include(d => d.PurchaseReturn)
                .ToListAsync();
        }

        public async Task AddAsync(PurchaseReturnDetail entity)
        {
            await _context.PurchaseReturnDetails.AddAsync(entity);
        }

        public Task UpdateAsync(PurchaseReturnDetail entity)
        {
            _context.PurchaseReturnDetails.Update(entity);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
             var entity = await GetByIdAsync(id);
             if (entity != null) _context.PurchaseReturnDetails.Remove(entity);
        }

        public async Task SoftDeleteAsync(int id)
        {
             var entity = await GetByIdAsync(id);
             if (entity != null) _context.PurchaseReturnDetails.Remove(entity);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PurchaseReturnDetails.AnyAsync(x => x.Id == id);
        }
    }
}
