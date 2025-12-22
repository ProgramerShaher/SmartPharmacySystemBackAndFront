using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Financial;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

public class FinancialService : IFinancialService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<FinancialService> _logger;

    public FinancialService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<FinancialService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PharmacyAccountDto> GetBalanceAsync()
    {
        var account = await _unitOfWork.Financials.GetMainAccountAsync();
        return _mapper.Map<PharmacyAccountDto>(account);
    }

    public async Task<FinancialTransactionDto> ProcessTransactionAsync(
        decimal amount,
        FinancialTransactionType type,
        string description,
        int? originalInvoiceId = null,
        FinancialInvoiceType? invoiceType = null)
    {
        _logger.LogInformation("Processing {Type} transaction: {Amount}. Description: {Description}", type, amount, description);

        // 1. Update Account Balance
        var account = await _unitOfWork.Financials.GetMainAccountAsync();
        if (type == FinancialTransactionType.Income)
        {
            account.Balance += amount;
        }
        else
        {
            account.Balance -= amount;
        }
        account.LastUpdated = DateTime.UtcNow;
        await _unitOfWork.Financials.UpdateAccountAsync(account);

        // 2. Handle Financial Invoice if provided
        int? financialInvoiceId = null;
        if (originalInvoiceId.HasValue && invoiceType.HasValue)
        {
            var financialInvoice = new FinancialInvoice
            {
                Type = invoiceType.Value,
                TotalAmount = amount,
                Date = DateTime.UtcNow,
                OriginalInvoiceId = originalInvoiceId.Value
            };
            await _unitOfWork.Financials.AddFinancialInvoiceAsync(financialInvoice);
            await _unitOfWork.SaveChangesAsync(); // Need Id for transaction relation
            financialInvoiceId = financialInvoice.Id;
        }

        // 3. Record Transaction
        var transaction = new FinancialTransaction
        {
            Type = type,
            Amount = amount,
            Description = description,
            Date = DateTime.UtcNow,
            RelatedInvoiceId = financialInvoiceId
        };
        await _unitOfWork.Financials.AddTransactionAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Financial transaction completed successfully. New balance: {Balance}", account.Balance);
        return _mapper.Map<FinancialTransactionDto>(transaction);
    }

    public async Task<PagedResponse<FinancialTransactionDto>> GetTransactionsAsync(FinancialTransactionQueryDto query)
    {
        var skip = (query.Page - 1) * query.PageSize;
        var items = await _unitOfWork.Financials.GetTransactionsAsync(query.StartDate, query.EndDate, query.Type, skip, query.PageSize);
        var total = await _unitOfWork.Financials.GetTransactionsCountAsync(query.StartDate, query.EndDate, query.Type);

        return new PagedResponse<FinancialTransactionDto>(
            _mapper.Map<IEnumerable<FinancialTransactionDto>>(items),
            total,
            query.Page,
            query.PageSize);
    }

    public async Task<PagedResponse<FinancialInvoiceDto>> GetFinancialInvoicesAsync(FinancialInvoiceQueryDto query)
    {
        var skip = (query.Page - 1) * query.PageSize;
        var items = await _unitOfWork.Financials.GetFinancialInvoicesAsync(query.StartDate, query.EndDate, query.Type, skip, query.PageSize);
        var total = await _unitOfWork.Financials.GetFinancialInvoicesCountAsync(query.StartDate, query.EndDate, query.Type);

        return new PagedResponse<FinancialInvoiceDto>(
            _mapper.Map<IEnumerable<FinancialInvoiceDto>>(items),
            total,
            query.Page,
            query.PageSize);
    }

    public async Task<FinancialReportDto> GetFinancialReportAsync(DateTime? start, DateTime? end)
    {
        var income = await _unitOfWork.Financials.GetTotalByTransactionTypeAsync(FinancialTransactionType.Income, start, end);
        var expense = await _unitOfWork.Financials.GetTotalByTransactionTypeAsync(FinancialTransactionType.Expense, start, end);
        var account = await _unitOfWork.Financials.GetMainAccountAsync();

        return new FinancialReportDto
        {
            TotalIncome = income,
            TotalExpense = expense,
            NetProfit = income - expense,
            StartDate = start ?? DateTime.MinValue,
            EndDate = end ?? DateTime.UtcNow,
            CurrentBalance = account.Balance
        };
    }
}
