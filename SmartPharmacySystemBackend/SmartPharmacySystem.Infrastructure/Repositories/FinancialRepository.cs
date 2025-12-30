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
        return await _context.PharmacyAccounts.FirstOrDefaultAsync(a => a.Id == 1 && a.IsActive)
               ?? throw new InvalidOperationException("Pharmacy account not found.");
    }

    public async Task<PharmacyAccount?> GetAccountByIdAsync(int id)
    {
        return await _context.PharmacyAccounts.FirstOrDefaultAsync(a => a.Id == id);
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

        if (start.HasValue) query = query.Where(t => t.CreatedAt >= start.Value);
        if (end.HasValue) query = query.Where(t => t.CreatedAt <= end.Value);
        if (type.HasValue) query = query.Where(t => t.Type == type.Value);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetTransactionsCountAsync(DateTime? start, DateTime? end, FinancialTransactionType? type)
    {
        var query = _context.FinancialTransactions.AsQueryable();

        if (start.HasValue) query = query.Where(t => t.CreatedAt >= start.Value);
        if (end.HasValue) query = query.Where(t => t.CreatedAt <= end.Value);
        if (type.HasValue) query = query.Where(t => t.Type == type.Value);

        return await query.CountAsync();
    }

    public async Task<decimal> GetTotalByTransactionTypeAsync(FinancialTransactionType type, DateTime? start, DateTime? end)
    {
        var query = _context.FinancialTransactions.Where(t => t.Type == type);

        if (start.HasValue) query = query.Where(t => t.CreatedAt >= start.Value);
        if (end.HasValue) query = query.Where(t => t.CreatedAt <= end.Value);

        return await query.SumAsync(t => (decimal?)t.Amount) ?? 0;
    }

    public async Task<IEnumerable<FinancialTransaction>> GetTransactionsByReferenceAsync(ReferenceType referenceType, int? referenceId = null)
    {
        var query = _context.FinancialTransactions.Where(t => t.ReferenceType == referenceType);

        if (referenceId.HasValue)
        {
            query = query.Where(t => t.ReferenceId == referenceId.Value);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// حساب الرصيد من الحركات المالية فقط
    /// Calculate balance from financial transactions only
    /// </summary>
    public async Task<decimal> CalculateBalanceAsync(int accountId)
    {
        var income = await _context.FinancialTransactions
            .Where(t => t.AccountId == accountId && t.Type == FinancialTransactionType.Income)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var expense = await _context.FinancialTransactions
            .Where(t => t.AccountId == accountId && t.Type == FinancialTransactionType.Expense)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        return income - expense;
    }

    /// <summary>
    /// حساب الرصيد حتى تاريخ معين
    /// Calculate balance up to a specific date
    /// </summary>
    public async Task<decimal> CalculateBalanceAsync(int accountId, DateTime upToDate)
    {
        var income = await _context.FinancialTransactions
            .Where(t => t.AccountId == accountId
                     && t.Type == FinancialTransactionType.Income
                     && t.TransactionDate <= upToDate)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var expense = await _context.FinancialTransactions
            .Where(t => t.AccountId == accountId
                     && t.Type == FinancialTransactionType.Expense
                     && t.TransactionDate <= upToDate)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        return income - expense;
    }

    public async Task<IEnumerable<FinancialTransaction>> GetTransactionsByAccountAsync(int accountId)
    {
        return await _context.FinancialTransactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<FinancialTransaction>> GetTransactionsByAccountAndDateRangeAsync(int accountId, DateTime start, DateTime end)
    {
        return await _context.FinancialTransactions
            .Where(t => t.AccountId == accountId
                     && t.TransactionDate >= start
                     && t.TransactionDate <= end)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    /// <summary>
    /// تفصيل الإيرادات حسب المصدر
    /// Income breakdown by source
    /// </summary>
    public async Task<Dictionary<ReferenceType, decimal>> GetIncomeBreakdownAsync(DateTime? start, DateTime? end)
    {
        var query = _context.FinancialTransactions
            .Where(t => t.Type == FinancialTransactionType.Income);

        if (start.HasValue) query = query.Where(t => t.TransactionDate >= start.Value);
        if (end.HasValue) query = query.Where(t => t.TransactionDate <= end.Value);

        return await query
            .GroupBy(t => t.ReferenceType)
            .Select(g => new { ReferenceType = g.Key, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.ReferenceType, x => x.Total);
    }

    /// <summary>
    /// تفصيل المصروفات حسب النوع
    /// Expense breakdown by type
    /// </summary>
    public async Task<Dictionary<ReferenceType, decimal>> GetExpenseBreakdownAsync(DateTime? start, DateTime? end)
    {
        var query = _context.FinancialTransactions
            .Where(t => t.Type == FinancialTransactionType.Expense);

        if (start.HasValue) query = query.Where(t => t.TransactionDate >= start.Value);
        if (end.HasValue) query = query.Where(t => t.TransactionDate <= end.Value);

        return await query
            .GroupBy(t => t.ReferenceType)
            .Select(g => new { ReferenceType = g.Key, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.ReferenceType, x => x.Total);
    }

    public async Task<IEnumerable<FinancialTransaction>> GetTransactionsByReferenceTypeAndIdAsync(ReferenceType referenceType, int referenceId)
    {
        return await _context.FinancialTransactions
            .Where(t => t.ReferenceType == referenceType && t.ReferenceId == referenceId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<GeneralLedgerRawItem>> GetGeneralLedgerRawDataAsync(DateTime? start, DateTime? end)
    {
        var transactionsQuery = _context.FinancialTransactions.AsQueryable();

        if (start.HasValue) transactionsQuery = transactionsQuery.Where(t => t.TransactionDate >= start.Value);
        if (end.HasValue) transactionsQuery = transactionsQuery.Where(t => t.TransactionDate <= end.Value);

        var data = await (from t in transactionsQuery
                          join e in _context.Expenses on new { ReferenceId = t.ReferenceId, ReferenceType = t.ReferenceType } equals new { ReferenceId = e.Id, ReferenceType = ReferenceType.Expense } into expenseGroup
                          from eg in expenseGroup.DefaultIfEmpty()
                          join im in _context.InventoryMovements on new { ReferenceId = t.ReferenceId, ReferenceType = t.ReferenceType } equals new { ReferenceId = im.ReferenceId, ReferenceType = im.ReferenceType } into movementGroup
                          from mg in movementGroup.DefaultIfEmpty()
                          select new GeneralLedgerRawItem
                          {
                              TransactionDate = t.TransactionDate,
                              Description = t.Description,
                              Amount = t.Amount,
                              Type = t.Type,
                              ReferenceId = t.ReferenceId,
                              ReferenceType = t.ReferenceType,
                              ExpenseCategory = eg != null ? eg.Category.Name : null,
                              ReferenceNumber = mg != null ? mg.ReferenceNumber : (eg != null ? eg.Id.ToString() : t.ReferenceId.ToString())
                          })
                          .OrderBy(t => t.TransactionDate)
                          .ToListAsync();

        return data;
    }
}
