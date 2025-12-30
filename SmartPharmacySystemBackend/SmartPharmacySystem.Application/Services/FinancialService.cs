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

    /// <summary>
    /// معالجة حركة مالية واحدة
    /// CRITICAL: الرصيد يُحسب من الحركات فقط - لا تعديل مباشر
    /// إذا كانت Expense: التحقق من كفاية الرصيد قبل الخصم
    /// إذا كانت Income: زيادة الرصيد مباشرة
    /// </summary>
    public async Task<FinancialTransactionDto> ProcessTransactionAsync(
        int accountId,
        decimal amount,
        FinancialTransactionType type,
        ReferenceType referenceType,
        int referenceId,
        string description)
    {
        _logger.LogInformation("Processing {Type} transaction: {Amount}. Reference: {ReferenceType}:{ReferenceId}",
            type, amount, referenceType, referenceId);

        // 1. الحصول على الحساب
        var account = await _unitOfWork.Financials.GetAccountByIdAsync(accountId)
            ?? throw new KeyNotFoundException($"الحساب برقم {accountId} غير موجود");

        if (!account.IsActive)
            throw new InvalidOperationException("الحساب غير نشط");

        // 2. حساب الرصيد الحالي من الحركات المالية
        var currentBalance = await _unitOfWork.Financials.CalculateBalanceAsync(accountId);

        // 3. التحقق من كفاية الرصيد إذا كان خصم
        if (type == FinancialTransactionType.Expense)
        {
            if (currentBalance < amount)
            {
                throw new InvalidOperationException(
                    $"الرصيد غير كافٍ. الرصيد الحالي: {currentBalance:N2} ريال، المطلوب: {amount:N2} ريال");
            }
        }

        // 4. إنشاء الحركة المالية
        var transaction = new FinancialTransaction
        {
            AccountId = accountId,
            Amount = amount,
            Type = type,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Description = description,
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Financials.AddTransactionAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        // 5. إعادة حساب وتحديث الرصيد من الحركات
        var newBalance = await _unitOfWork.Financials.CalculateBalanceAsync(accountId);
        account.Balance = newBalance;
        account.LastUpdated = DateTime.UtcNow;
        await _unitOfWork.Financials.UpdateAccountAsync(account);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Financial transaction completed. New balance: {Balance}", newBalance);
        return _mapper.Map<FinancialTransactionDto>(transaction);
    }

    /// <summary>
    /// إضافة رصيد افتتاحي - مرة واحدة فقط
    /// Opening balance should only be added once
    /// </summary>
    public async Task<FinancialTransactionDto> AddOpeningBalanceAsync(
        int accountId,
        decimal amount,
        string description = "الرصيد الافتتاحي")
    {
        _logger.LogInformation("Adding opening balance: {Amount} to account {AccountId}", amount, accountId);

        // التحقق من عدم وجود opening balance سابق
        var existingOpeningBalance = await _unitOfWork.Financials
            .GetTransactionsByReferenceAsync(ReferenceType.OpeningBalance);

        if (existingOpeningBalance.Any())
        {
            throw new InvalidOperationException("الرصيد الافتتاحي موجود مسبقاً. لا يمكن إضافة رصيد افتتاحي أكثر من مرة");
        }

        return await ProcessTransactionAsync(
            accountId,
            amount,
            FinancialTransactionType.Income,
            ReferenceType.OpeningBalance,
            0, // No specific reference ID for opening balance
            description
        );
    }

    /// <summary>
    /// إضافة تعديل مالي يدوي (Admin فقط)
    /// Add manual financial adjustment (Admin only)
    /// يستخدم لمعالجة: فرق جرد، كاش خارج النظام، تصحيح أخطاء
    /// </summary>
    public async Task<FinancialTransactionDto> AddManualAdjustmentAsync(
        int accountId,
        decimal amount,
        string description,
        bool isAdminUser = false)
    {
        // التحقق من الصلاحيات
        if (!isAdminUser)
        {
            throw new UnauthorizedAccessException("التعديلات اليدوية مسموحة للمدير فقط");
        }

        // التحقق من وجود سبب
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("السبب (Description) إلزامي للتعديلات اليدوية");
        }

        _logger.LogWarning("Manual adjustment: {Amount} to account {AccountId}. Reason: {Description}",
            amount, accountId, description);

        // تحديد نوع الحركة بناءً على المبلغ
        var transactionType = amount >= 0
            ? FinancialTransactionType.Income
            : FinancialTransactionType.Expense;

        return await ProcessTransactionAsync(
            accountId,
            Math.Abs(amount), // Always store positive amount
            transactionType,
            ReferenceType.ManualAdjustment,
            0, // No specific reference ID
            $"تعديل يدوي: {description}"
        );
    }

    public async Task<PagedResponse<FinancialTransactionDto>> GetTransactionsAsync(FinancialTransactionQueryDto query)
    {
        var skip = (query.Page - 1) * query.PageSize;
        var items = await _unitOfWork.Financials.GetTransactionsAsync(
            query.StartDate, query.EndDate, query.Type, skip, query.PageSize);
        var total = await _unitOfWork.Financials.GetTransactionsCountAsync(
            query.StartDate, query.EndDate, query.Type);

        return new PagedResponse<FinancialTransactionDto>(
            _mapper.Map<IEnumerable<FinancialTransactionDto>>(items),
            total,
            query.Page,
            query.PageSize);
    }

    public async Task<FinancialReportDto> GetFinancialReportAsync(DateTime? start, DateTime? end)
    {
        var income = await _unitOfWork.Financials.GetTotalByTransactionTypeAsync(
            FinancialTransactionType.Income, start, end);
        var expense = await _unitOfWork.Financials.GetTotalByTransactionTypeAsync(
            FinancialTransactionType.Expense, start, end);
        var account = await _unitOfWork.Financials.GetMainAccountAsync();

        // تحديد تاريخ البداية
        DateTime reportStart;
        if (start.HasValue)
        {
            reportStart = start.Value;
        }
        else
        {
            // استخدام الدالة الموجودة لديك لجلب أول حركة (نأخذ أقدم حركة)
            // نمرر معاملات واسعة لجلب أقدم شيء مسجل
            var transactions = await _unitOfWork.Financials.GetTransactionsAsync(null, null, null, 0, 1000);

            // إذا وجدنا عمليات، نأخذ تاريخ أقدم واحدة، وإذا لم نجد نستخدم بداية الشهر الحالي
            reportStart = transactions.Any()
                ? transactions.Min(t => t.CreatedAt)
                : new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        }

        return new FinancialReportDto
        {
            TotalIncome = income,
            TotalExpense = expense,
            NetProfit = income - expense,
            StartDate = reportStart, // سيظهر الآن تاريخ حقيقي
            EndDate = end ?? DateTime.UtcNow,
            CurrentBalance = account.Balance
        };
    }
    public async Task<PagedResponse<GeneralLedgerDto>> GetGeneralLedgerAsync(DateTime? start, DateTime? end, int page, int pageSize)
    {
        // 1. Get raw data from repository
        var rawData = await _unitOfWork.Financials.GetGeneralLedgerRawDataAsync(start, end);

        decimal runningBalance = 0;
        var ledgers = new List<GeneralLedgerDto>();

        // 2. Calculate running balance for ALL retrieved records
        foreach (var item in rawData)
        {
            var amount = item.Amount;
            var type = item.Type;

            if (type == FinancialTransactionType.Income)
                runningBalance += amount;
            else
                runningBalance -= amount;

            ledgers.Add(new GeneralLedgerDto
            {
                TransactionDate = item.TransactionDate,
                Description = item.Description,
                Category = item.ExpenseCategory ?? (type == FinancialTransactionType.Income ? "إيراد" : "مصروف"),
                Incoming = type == FinancialTransactionType.Income ? amount : 0,
                Outgoing = type == FinancialTransactionType.Expense ? amount : 0,
                RunningBalance = runningBalance,
                ReferenceId = item.ReferenceId,
                ReferenceType = item.ReferenceType,
                ReferenceNumber = item.ReferenceNumber
            });
        }

        // 3. Apply pagination on the calculated list
        var total = ledgers.Count;
        var pagedData = ledgers
            .OrderByDescending(l => l.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResponse<GeneralLedgerDto>(pagedData, total, page, pageSize);
    }

    public async Task<IEnumerable<AnnualFinancialReportDto>> GetAnnualFinancialReportAsync(int year)
    {
        var start = new DateTime(year, 1, 1);
        var end = new DateTime(year, 12, 31, 23, 59, 59);

        // We can use the existing raw data query if we don't want to add a specialized repository method
        var rawData = await _unitOfWork.Financials.GetGeneralLedgerRawDataAsync(start, end);

        var report = rawData
            .GroupBy(item => item.ExpenseCategory ?? (item.Type == FinancialTransactionType.Income ? "إجمالي الإيرادات" : "مصروفات عامة"))
            .Select(g => new AnnualFinancialReportDto
            {
                CategoryName = g.Key,
                TotalAmount = g.Sum(x => x.Amount),
                TransactionCount = g.Count()
            })
            .ToList();

        return report;
    }

    public async Task<IEnumerable<FinancialSummaryDto>> GetAnnualFinancialSummaryAsync(int year)
    {
        var start = new DateTime(year, 1, 1);
        var end = new DateTime(year, 12, 31, 23, 59, 59);

        var rawData = await _unitOfWork.Financials.GetGeneralLedgerRawDataAsync(start, end);

        // Smart Category Resolver
        string ResolveCategory(GeneralLedgerRawItem item)
        {
            if (!string.IsNullOrEmpty(item.ExpenseCategory))
                return item.ExpenseCategory;

            var desc = item.Description.ToLower();
            if (desc.Contains("كهرباء") || desc.Contains("ماء") || desc.Contains("هاتف") || desc.Contains("انترنت"))
                return "المرافق والخدمات";
            if (desc.Contains("إيجار") || desc.Contains("ايجار"))
                return "الإيجارات";
            if (desc.Contains("رواتب") || desc.Contains("راتب"))
                return "الرواتب والأجور";
            if (desc.Contains("نظافة") || desc.Contains("بلدية"))
                return "رسوم حكومية ونظافة";

            if (item.ReferenceType == ReferenceType.SaleInvoice || item.ReferenceType == ReferenceType.SalesReturn)
                return "نشاط مبيعات";
            if (item.ReferenceType == ReferenceType.PurchaseInvoice || item.ReferenceType == ReferenceType.PurchaseReturn || item.ReferenceType == ReferenceType.SupplierPayment)
                return "نشاط مشتريات";

            return item.Type == FinancialTransactionType.Income ? "إيرادات أخرى" : "مصاريف عامة";
        }

        var groupedData = rawData
            .GroupBy(ResolveCategory)
            .Select(g => new FinancialSummaryDto
            {
                CategoryName = g.Key,
                TotalAmount = g.Sum(x => x.Amount),
                TransactionCount = g.Count()
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

        // Calculate Percentages
        var totalAll = groupedData.Sum(x => x.TotalAmount);
        if (totalAll > 0)
        {
            foreach (var item in groupedData)
            {
                item.Percentage = (double)(item.TotalAmount / totalAll * 100);
                item.Percentage = Math.Round(item.Percentage, 2);
            }
        }

        return groupedData;
    }

    public async Task ReverseFinancialTransactionAsync(ReferenceType referenceType, int referenceId, string description)
    {
        var transactions = await _unitOfWork.Financials.GetTransactionsByReferenceAsync(referenceType);
        var lastTransaction = transactions.OrderByDescending(t => t.TransactionDate)
                                         .FirstOrDefault(t => t.ReferenceId == referenceId);

        if (lastTransaction != null)
        {
            var reverseType = lastTransaction.Type == FinancialTransactionType.Income
                ? FinancialTransactionType.Expense
                : FinancialTransactionType.Income;

            await ProcessTransactionAsync(
                accountId: lastTransaction.AccountId,
                amount: lastTransaction.Amount,
                type: reverseType,
                referenceType: referenceType,
                referenceId: referenceId,
                description: description);
        }
    }
}
