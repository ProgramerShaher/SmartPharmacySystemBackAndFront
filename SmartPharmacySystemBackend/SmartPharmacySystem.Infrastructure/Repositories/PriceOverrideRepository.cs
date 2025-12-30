using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories
{
    public class PriceOverrideRepository : IPriceOverrideRepository
    {
        private readonly ApplicationDbContext _context;

        public PriceOverrideRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PriceOverride entity)
        {
            await _context.PriceOverrides.AddAsync(entity);
        }

        public async Task<IEnumerable<PriceOverride>> GetAllAsync()
        {
            return await _context.PriceOverrides
                .Include(p => p.Medicine)
                .Include(p => p.Batch)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PriceOverride>> GetByMedicineIdAsync(int medicineId)
        {
            return await _context.PriceOverrides
                .Where(p => p.MedicineId == medicineId)
                .Include(p => p.Batch)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
