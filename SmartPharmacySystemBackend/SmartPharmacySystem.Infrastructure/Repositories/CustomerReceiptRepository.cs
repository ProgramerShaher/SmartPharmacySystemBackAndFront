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

        public async Task<(IEnumerable<CustomerReceipt> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.CustomerReceipts
                .Include(r => r.Customer)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Customer.Name.Contains(search) || r.Id.ToString().Contains(search) || (r.ReferenceNo != null && r.ReferenceNo.Contains(search)));
            }

            if (fromDate.HasValue)
                query = query.Where(r => r.ReceiptDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.ReceiptDate <= toDate.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.ReceiptDate)
                .ThenByDescending(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
        public async Task<(int TotalCount, decimal TotalAmount, decimal TodayAmount)> GetStatisticsAsync()
        {
            var today = DateTime.UtcNow.Date;
            
            var totalCount = await _context.CustomerReceipts
                .CountAsync(r => !r.IsCancelled);

            var totalAmount = await _context.CustomerReceipts
                .Where(r => !r.IsCancelled)
                .SumAsync(r => r.Amount);

            var todayAmount = await _context.CustomerReceipts
                .Where(r => !r.IsCancelled && r.ReceiptDate.Date == today)
                .SumAsync(r => r.Amount);

            return (totalCount, totalAmount, todayAmount);
        }
    }
}
