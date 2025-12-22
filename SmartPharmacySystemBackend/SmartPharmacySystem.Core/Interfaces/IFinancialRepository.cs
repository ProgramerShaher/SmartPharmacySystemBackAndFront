using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Core.Interfaces;

public interface IFinancialRepository
{
    // Account operations
    Task<PharmacyAccount> GetMainAccountAsync();
    Task UpdateAccountAsync(PharmacyAccount account);

    // Transaction operations
    Task AddTransactionAsync(FinancialTransaction transaction);
    Task<IEnumerable<FinancialTransaction>> GetTransactionsAsync(DateTime? start, DateTime? end, FinancialTransactionType? type, int skip, int take);
    Task<int> GetTransactionsCountAsync(DateTime? start, DateTime? end, FinancialTransactionType? type);
    Task<decimal> GetTotalByTransactionTypeAsync(FinancialTransactionType type, DateTime? start, DateTime? end);

    // Invoice operations
    Task AddFinancialInvoiceAsync(FinancialInvoice invoice);
    Task<IEnumerable<FinancialInvoice>> GetFinancialInvoicesAsync(DateTime? start, DateTime? end, FinancialInvoiceType? type, int skip, int take);
    Task<int> GetFinancialInvoicesCountAsync(DateTime? start, DateTime? end, FinancialInvoiceType? type);
}
