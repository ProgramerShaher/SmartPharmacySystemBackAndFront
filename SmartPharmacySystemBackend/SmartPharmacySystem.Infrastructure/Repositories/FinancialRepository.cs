using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;

namespace SmartPharmacySystem.Infrastructure.Repositories;

public class FinancialRepository : IFinancialRepository
{
    private readonly ApplicationDbContext _context;

    public FinancialRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PharmacyAccount> GetMainAccountAsync()
    {
        // Account with ID 1 is the main pharmacy account seeded in DbContext
        return await _context.PharmacyAccounts.FirstOrDefaultAsync(a => a.Id == 1)
               ?? throw new InvalidOperationException("Pharmacy account not found.");
    }

    public async Task UpdateAccountAsync(PharmacyAccount account)
    {
        _context.PharmacyAccounts.Update(account);
        await Task.CompletedTask;
    }

    public async Task AddTransactionAsync(FinancialTransaction transaction)
    {
        await _context.FinancialTransactions.AddAsync(transaction);
    }

    public async Task<IEnumerable<FinancialTransaction>> GetTransactionsAsync(DateTime? start, DateTime? end, FinancialTransactionType? type, int skip, int take)
    {
        var query = _context.FinancialTransactions.AsQueryable();

        if (start.HasValue) query = query.Where(t => t.Date >= start.Value);
        if (end.HasValue) query = query.Where(t => t.Date <= end.Value);
        if (type.HasValue) query = query.Where(t => t.Type == type.Value);

        return await query
            .OrderByDescending(t => t.Date)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetTransactionsCountAsync(DateTime? start, DateTime? end, FinancialTransactionType? type)
    {
        var query = _context.FinancialTransactions.AsQueryable();

        if (start.HasValue) query = query.Where(t => t.Date >= start.Value);
        if (end.HasValue) query = query.Where(t => t.Date <= end.Value);
        if (type.HasValue) query = query.Where(t => t.Type == type.Value);

        return await query.CountAsync();
    }

    public async Task<decimal> GetTotalByTransactionTypeAsync(FinancialTransactionType type, DateTime? start, DateTime? end)
    {
        var query = _context.FinancialTransactions.Where(t => t.Type == type);

        if (start.HasValue) query = query.Where(t => t.Date >= start.Value);
        if (end.HasValue) query = query.Where(t => t.Date <= end.Value);

        return await query.SumAsync(t => t.Amount);
    }

    public async Task AddFinancialInvoiceAsync(FinancialInvoice invoice)
    {
        await _context.FinancialInvoices.AddAsync(invoice);
    }

    public async Task<IEnumerable<FinancialInvoice>> GetFinancialInvoicesAsync(DateTime? start, DateTime? end, FinancialInvoiceType? type, int skip, int take)
    {
        var query = _context.FinancialInvoices.AsQueryable();

        if (start.HasValue) query = query.Where(i => i.Date >= start.Value);
        if (end.HasValue) query = query.Where(i => i.Date <= end.Value);
        if (type.HasValue) query = query.Where(i => i.Type == type.Value);

        return await query
            .OrderByDescending(i => i.Date)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetFinancialInvoicesCountAsync(DateTime? start, DateTime? end, FinancialInvoiceType? type)
    {
        var query = _context.FinancialInvoices.AsQueryable();

        if (start.HasValue) query = query.Where(i => i.Date >= start.Value);
        if (end.HasValue) query = query.Where(i => i.Date <= end.Value);
        if (type.HasValue) query = query.Where(i => i.Type == type.Value);

        return await query.CountAsync();
    }
}
