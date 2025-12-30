using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories
{
    public class CustomerReceiptRepository : ICustomerReceiptRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerReceiptRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerReceipt?> GetByIdAsync(int id)
        {
            return await _context.CustomerReceipts.FindAsync(id);
        }

        public async Task AddAsync(CustomerReceipt entity)
        {
            await _context.CustomerReceipts.AddAsync(entity);
        }

        public Task UpdateAsync(CustomerReceipt entity)
        {
            _context.CustomerReceipts.Update(entity);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<CustomerReceipt>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.CustomerReceipts
                .Where(r => r.CustomerId == customerId && !r.IsCancelled)
                .OrderByDescending(r => r.ReceiptDate)
                .ToListAsync();
        }

        public async Task<CustomerReceipt?> GetByIdWithCustomerAsync(int id)
        {
            return await _context.CustomerReceipts
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
