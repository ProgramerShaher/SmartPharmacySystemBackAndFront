using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories
{
    public class SalesReturnDetailRepository : ISalesReturnDetailRepository
    {
        private readonly ApplicationDbContext _context;

        public SalesReturnDetailRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SalesReturnDetail?> GetByIdAsync(int id)
        {
            return await _context.SalesReturnDetails
                .Include(d => d.Medicine)
                .Include(d => d.Batch)
                .Include(d => d.SalesReturn)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<SalesReturnDetail>> GetAllAsync()
        {
            return await _context.SalesReturnDetails
                .Include(d => d.Medicine)
                .Include(d => d.Batch)
                .Include(d => d.SalesReturn)
                .ToListAsync();
        }

        public async Task AddAsync(SalesReturnDetail entity)
        {
            await _context.SalesReturnDetails.AddAsync(entity);
        }

        public Task UpdateAsync(SalesReturnDetail entity)
        {
            _context.SalesReturnDetails.Update(entity);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
             var entity = await GetByIdAsync(id);
             if (entity != null) _context.SalesReturnDetails.Remove(entity);
        }

        public async Task SoftDeleteAsync(int id)
        {
             var entity = await GetByIdAsync(id);
             if (entity != null) _context.SalesReturnDetails.Remove(entity);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.SalesReturnDetails.AnyAsync(x => x.Id == id);
        }
    }
}
