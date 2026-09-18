using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Infrastructure.Repositories
{
    public class HeldInvoiceRepository : IHeldInvoiceRepository
    {
        private readonly ApplicationDbContext _context;

        public HeldInvoiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HeldInvoice?> GetByIdAsync(int id)
        {
            return await _context.HeldInvoices
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);
        }

        public async Task<IEnumerable<HeldInvoice>> GetAllActiveAsync(int? branchId = null)
        {
            var query = _context.HeldInvoices
                .AsNoTracking()
                .Where(h => !h.IsDeleted);

            if (branchId.HasValue)
            {
                query = query.Where(h => h.BranchId == branchId.Value);
            }

            return await query
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(HeldInvoice entity)
        {
            await _context.HeldInvoices.AddAsync(entity);
        }

        public async Task UpdateAsync(HeldInvoice entity)
        {
            _context.HeldInvoices.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.HeldInvoices.FindAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                _context.HeldInvoices.Update(entity);
            }
        }
    }
}
