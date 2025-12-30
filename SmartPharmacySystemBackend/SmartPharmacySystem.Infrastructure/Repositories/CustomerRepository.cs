using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.SaleInvoices)
                .Include(c => c.Receipts)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task AddAsync(Customer entity)
        {
            await _context.Customers.AddAsync(entity);
        }

        public Task UpdateAsync(Customer entity)
        {
            _context.Customers.Update(entity);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Customers.FindAsync(id);
            if (entity != null)
            {
                _context.Customers.Remove(entity);
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Customers.AnyAsync(c => c.Id == id);
        }

        public async Task<(IEnumerable<Customer> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(search) ||
                                         (c.PhoneNumber != null && c.PhoneNumber.Contains(search)));
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<Customer>> GetTopDebtorsAsync(int count)
        {
            return await _context.Customers
                .Where(c => c.Balance > 0 && c.IsActive)
                .OrderByDescending(c => c.Balance)
                .Take(count)
                .ToListAsync();
        }

        public async Task UpdateBalanceAsync(int customerId, decimal amount)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null)
            {
                customer.Balance += amount;
                _context.Entry(customer).State = EntityState.Modified;
            }
        }
    }
}
