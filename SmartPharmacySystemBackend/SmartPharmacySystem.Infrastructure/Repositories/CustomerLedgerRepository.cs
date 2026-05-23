using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class CustomerLedgerRepository : ICustomerLedgerRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerLedgerRepository(ApplicationDbContext context) => _context = context;

    public async Task<CustomerLedger?> GetByIdAsync(int id)
    {
        return await _context.CustomerLedgers
            .AsNoTracking()
            .Include(e => e.Customer)
            .Include(e => e.Branch)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<IEnumerable<CustomerLedger>> GetByCustomerIdAsync(int customerId, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var query = _context.CustomerLedgers.AsNoTracking()
            .Include(e => e.Branch)
            .Where(e => e.CustomerId == customerId && !e.IsDeleted);

        if (dateFrom.HasValue)
            query = query.Where(e => e.TransactionDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(e => e.TransactionDate <= dateTo.Value);

        return await query.OrderBy(e => e.TransactionDate).ToListAsync();
    }

    public async Task<IEnumerable<CustomerLedger>> GetByBranchIdAsync(int branchId, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var query = _context.CustomerLedgers.AsNoTracking()
            .Include(e => e.Customer)
            .Where(e => e.BranchId == branchId && !e.IsDeleted);

        if (dateFrom.HasValue)
            query = query.Where(e => e.TransactionDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(e => e.TransactionDate <= dateTo.Value);

        return await query.OrderBy(e => e.TransactionDate).ToListAsync();
    }

    public async Task<decimal> GetCustomerBalanceAsync(int customerId)
    {
        var totalDebit = await _context.CustomerLedgers
            .AsNoTracking()
            .Where(e => e.CustomerId == customerId && !e.IsDeleted)
            .SumAsync(e => e.Debit);

        var totalCredit = await _context.CustomerLedgers
            .AsNoTracking()
            .Where(e => e.CustomerId == customerId && !e.IsDeleted)
            .SumAsync(e => e.Credit);

        return totalDebit - totalCredit;
    }

    public async Task<decimal> GetCustomerBalanceAtBranchAsync(int customerId, int branchId)
    {
        var totalDebit = await _context.CustomerLedgers
            .AsNoTracking()
            .Where(e => e.CustomerId == customerId && e.BranchId == branchId && !e.IsDeleted)
            .SumAsync(e => e.Debit);

        var totalCredit = await _context.CustomerLedgers
            .AsNoTracking()
            .Where(e => e.CustomerId == customerId && e.BranchId == branchId && !e.IsDeleted)
            .SumAsync(e => e.Credit);

        return totalDebit - totalCredit;
    }

    public async Task<CustomerLedger> AddAsync(CustomerLedger entry)
    {
        await _context.CustomerLedgers.AddAsync(entry);
        return entry;
    }

    public async Task DeleteAsync(int id)
    {
        var entry = await _context.CustomerLedgers.FindAsync(id);
        if (entry != null)
        {
            entry.IsDeleted = true;
            _context.CustomerLedgers.Update(entry);
        }
    }

    public async Task<IEnumerable<CustomerLedger>> SearchAsync(int? customerId = null, int? branchId = null, CustomerTransactionType? type = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var query = _context.CustomerLedgers.AsNoTracking().Where(e => !e.IsDeleted).AsQueryable();

        if (customerId.HasValue)
            query = query.Where(e => e.CustomerId == customerId.Value);

        if (branchId.HasValue)
            query = query.Where(e => e.BranchId == branchId.Value);

        if (type.HasValue)
            query = query.Where(e => e.TransactionType == type.Value);

        if (dateFrom.HasValue)
            query = query.Where(e => e.TransactionDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(e => e.TransactionDate <= dateTo.Value);

        return await query
            .Include(e => e.Customer)
            .Include(e => e.Branch)
            .OrderByDescending(e => e.TransactionDate)
            .ToListAsync();
    }
}
