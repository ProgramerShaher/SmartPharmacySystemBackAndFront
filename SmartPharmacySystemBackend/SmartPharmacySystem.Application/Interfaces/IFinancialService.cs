using SmartPharmacySystem.Application.DTOs.Financial;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IFinancialService
{
    // Account operations
    Task<PharmacyAccountDto> GetBalanceAsync();

    // Core logic
    Task<FinancialTransactionDto> ProcessTransactionAsync(decimal amount, FinancialTransactionType type, string description, int? originalInvoiceId = null, FinancialInvoiceType? invoiceType = null);

    // Query operations
    Task<PagedResponse<FinancialTransactionDto>> GetTransactionsAsync(FinancialTransactionQueryDto query);
    Task<PagedResponse<FinancialInvoiceDto>> GetFinancialInvoicesAsync(FinancialInvoiceQueryDto query);

    // Reporting
    Task<FinancialReportDto> GetFinancialReportAsync(DateTime? start, DateTime? end);
}
