using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Infrastructure.Repositories
{
    public class ProductSerialNumberRepository : IProductSerialNumberRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductSerialNumberRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProductSerialNumber?> GetByIdAsync(int id)
        {
            return await _context.ProductSerialNumbers
                .Include(s => s.Medicine)
                .Include(s => s.Customer)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<ProductSerialNumber?> GetBySerialNumberAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber)) return null;

            return await _context.ProductSerialNumbers
                .Include(s => s.Medicine)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.SerialNumber.ToLower() == serialNumber.Trim().ToLower() && !s.IsDeleted);
        }

        public async Task<IEnumerable<ProductSerialNumber>> GetInStockByMedicineIdAsync(int medicineId)
        {
            return await _context.ProductSerialNumbers
                .AsNoTracking()
                .Where(s => s.MedicineId == medicineId &&
                            s.Status == SerialNumberStatus.InStock &&
                            !s.IsDeleted)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductSerialNumber>> GetBySaleDetailIdAsync(int saleDetailId)
        {
            return await _context.ProductSerialNumbers
                .AsNoTracking()
                .Where(s => s.SaleInvoiceDetailId == saleDetailId && !s.IsDeleted)
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<ProductSerialNumber> entities)
        {
            await _context.ProductSerialNumbers.AddRangeAsync(entities);
        }

        public async Task UpdateAsync(ProductSerialNumber entity)
        {
            _context.ProductSerialNumbers.Update(entity);
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber)) return false;

            return await _context.ProductSerialNumbers
                .AnyAsync(s => s.SerialNumber.ToLower() == serialNumber.Trim().ToLower() && !s.IsDeleted);
        }
    }
}
