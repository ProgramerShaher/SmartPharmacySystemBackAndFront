using SmartPharmacySystem.Application.DTOs.Financial;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.Interfaces;

public interface IFinancialService
{
    // Account operations
    Task<PharmacyAccountDto> GetBalanceAsync();

    // Core transaction processing
    Task<FinancialTransactionDto> ProcessTransactionAsync(
        int accountId,
        decimal amount,
        FinancialTransactionType type,
        ReferenceType referenceType,
        int referenceId,
        string description);

    Task<FinancialTransactionDto> AddOpeningBalanceAsync(
        int accountId,
        decimal amount,
        string description = "الرصيد الافتتاحي");

    Task<FinancialTransactionDto> AddManualAdjustmentAsync(
        int accountId,
        decimal amount,
        string description,
        bool isAdminUser = false);

    // Query operations
    Task<PagedResponse<FinancialTransactionDto>> GetTransactionsAsync(FinancialTransactionQueryDto query);

    // Reporting
    Task<FinancialReportDto> GetFinancialReportAsync(DateTime? start, DateTime? end);

    Task<PagedResponse<GeneralLedgerDto>> GetGeneralLedgerAsync(DateTime? start, DateTime? end, int page, int pageSize);
    Task<IEnumerable<AnnualFinancialReportDto>> GetAnnualFinancialReportAsync(int year);
    Task<IEnumerable<FinancialSummaryDto>> GetAnnualFinancialSummaryAsync(int year);
    Task ReverseFinancialTransactionAsync(ReferenceType referenceType, int referenceId, string description);
}
