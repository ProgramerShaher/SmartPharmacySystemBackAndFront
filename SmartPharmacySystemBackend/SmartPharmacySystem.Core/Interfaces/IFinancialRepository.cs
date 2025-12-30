using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IFinancialRepository
{
    // Account operations
    Task<PharmacyAccount> GetMainAccountAsync();
    Task<PharmacyAccount?> GetAccountByIdAsync(int id);
    Task UpdateAccountAsync(PharmacyAccount account);

    // Transaction operations
    Task AddTransactionAsync(FinancialTransaction transaction);
    Task<IEnumerable<FinancialTransaction>> GetTransactionsAsync(DateTime? start, DateTime? end, FinancialTransactionType? type, int skip, int take);
    Task<int> GetTransactionsCountAsync(DateTime? start, DateTime? end, FinancialTransactionType? type);
    Task<decimal> GetTotalByTransactionTypeAsync(FinancialTransactionType type, DateTime? start, DateTime? end);
    Task<IEnumerable<FinancialTransaction>> GetTransactionsByReferenceAsync(ReferenceType referenceType, int? referenceId = null);

    // Balance calculation from transactions
    Task<decimal> CalculateBalanceAsync(int accountId);
    Task<decimal> CalculateBalanceAsync(int accountId, DateTime upToDate);

    // Transaction queries by account
    Task<IEnumerable<FinancialTransaction>> GetTransactionsByAccountAsync(int accountId);
    Task<IEnumerable<FinancialTransaction>> GetTransactionsByAccountAndDateRangeAsync(int accountId, DateTime start, DateTime end);

    // Detailed reporting
    Task<Dictionary<ReferenceType, decimal>> GetIncomeBreakdownAsync(DateTime? start, DateTime? end);
    Task<Dictionary<ReferenceType, decimal>> GetExpenseBreakdownAsync(DateTime? start, DateTime? end);
    Task<IEnumerable<FinancialTransaction>> GetTransactionsByReferenceTypeAndIdAsync(ReferenceType referenceType, int referenceId);

    // General Ledger Query
    Task<IEnumerable<GeneralLedgerRawItem>> GetGeneralLedgerRawDataAsync(DateTime? start, DateTime? end);
}
