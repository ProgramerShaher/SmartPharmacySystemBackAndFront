using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Infrastructure.Repositories
{
    public class ProductVariantRepository : IProductVariantRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductVariantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProductVariant?> GetByIdAsync(int id)
        {
            return await _context.ProductVariants
                .Include(v => v.Medicine)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        }

        public async Task<IEnumerable<ProductVariant>> GetByMedicineIdAsync(int medicineId)
        {
            return await _context.ProductVariants
                .AsNoTracking()
                .Where(v => v.MedicineId == medicineId && !v.IsDeleted && v.IsActive)
                .OrderBy(v => v.Size)
                .ThenBy(v => v.Color)
                .ToListAsync();
        }

        public async Task<ProductVariant?> GetByBarcodeOrSkuAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            return await _context.ProductVariants
                .Include(v => v.Medicine)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => (!v.IsDeleted && v.IsActive) &&
                    (v.Barcode == code || v.Sku == code));
        }

        public async Task AddAsync(ProductVariant entity)
        {
            await _context.ProductVariants.AddAsync(entity);
        }

        public async Task UpdateAsync(ProductVariant entity)
        {
            _context.ProductVariants.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.ProductVariants.FindAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                _context.ProductVariants.Update(entity);
            }
        }
    }
}
