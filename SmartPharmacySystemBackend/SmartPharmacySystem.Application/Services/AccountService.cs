using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Financial;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

/// <summary>
/// تنفيذ خدمة إدارة شجرة الحسابات
/// </summary>
public class AccountService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<AccountService> logger) : IAccountService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<AccountService> _logger = logger;

    public async Task<IEnumerable<AccountDto>> GetAccountTreeAsync()
    {
        _logger.LogInformation("جاري جلب شجرة الحسابات بالكامل");
        var accounts = await _unitOfWork.Accounts.GetAllAsync();
        return _mapper.Map<IEnumerable<AccountDto>>(accounts);
    }

    public async Task<AccountDto> GetByIdAsync(int id)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"الحساب المحاسبي برقم {id} غير موجود");
        return _mapper.Map<AccountDto>(account);
    }

    public async Task<IEnumerable<AccountDto>> GetByAccountTypeAsync(string accountType)
    {
        var accounts = await _unitOfWork.Accounts.GetAllAsync();
        var filtered = accounts.Where(a => a.Type.ToString() == accountType);
        return _mapper.Map<IEnumerable<AccountDto>>(filtered);
    }

    public async Task<AccountDto> CreateAsync(AccountDto dto)
    {
        _logger.LogInformation("جاري إنشاء حساب جديد: {Name} بالكود {Code}", dto.Name, dto.Code);

        // التحقق من فرادة الكود
        if (await _unitOfWork.Accounts.CodeExistsAsync(dto.Code))
            throw new InvalidOperationException($"الكود المحاسبي {dto.Code} مستخدم بالفعل");

        var account = _mapper.Map<Account>(dto);
        account.CreatedAt = DateTime.UtcNow;
        account.IsActive = true;

        await _unitOfWork.Accounts.AddAsync(account);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AccountDto>(account);
    }

    public async Task UpdateAsync(AccountDto dto)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"الحساب برقم {dto.Id} غير موجود");

        _mapper.Map(dto, account);
        account.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Accounts.UpdateAsync(account);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"الحساب برقم {id} غير موجود");

        // 1. التحقق من وجود حسابات فرعية
        var children = await _unitOfWork.Accounts.GetChildrenAsync(id);
        if (children.Any())
            throw new InvalidOperationException("لا يمكن حذف حساب رئيسي يحتوي على حسابات فرعية. يرجى حذف أو نقل الحسابات الفرعية أولاً.");

        // 2. التحقق من وجود أرصدة مالية
        if (account.CurrentBalance != 0)
            throw new InvalidOperationException($"لا يمكن حذف الحساب ({account.Name}) لأن رصيده الحالي ليس صفراً ({account.CurrentBalance:N0}). يرجى تصفية الرصيد بقيد محاسبي أولاً.");

        // 3. التحقق من وجود أي حركات مالية سابقة (حتى لو الرصيد صفر)
        var hasTransactions = (await _unitOfWork.JournalEntries.GetLinesByAccountIdAsync(id, null, null)).Any();
        if (hasTransactions)
            throw new InvalidOperationException($"لا يمكن حذف الحساب ({account.Name}) لوجود حركات مالية مسجلة عليه في النظام. يمكنك إلغاء تفعيله بدلاً من الحذف إذا كان رصيده صفراً.");

        await _unitOfWork.Accounts.SoftDeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ToggleStatusAsync(int id, bool isActive)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"الحساب برقم {id} غير موجود");

        // إذا كان المستخدم يحاول إلغاء التفعيل (تعطيل الحساب)
        if (!isActive)
        {
            // 1. التحقق من الرصيد
            if (account.CurrentBalance != 0)
                throw new InvalidOperationException($"لا يمكن إلغاء تفعيل الحساب ({account.Name}) لأن رصيده الحالي ليس صفراً. يرجى تصفير الحساب قبل التعطيل.");

            // 2. التحقق من الحسابات الرئيسية
            if (account.IsMainAccount)
            {
                var children = await _unitOfWork.Accounts.GetChildrenAsync(id);
                if (children.Any(c => c.IsActive))
                    throw new InvalidOperationException("لا يمكن تعطيل حساب رئيسي لا تزال بعض حساباته الفرعية نشطة.");
            }
        }

        account.IsActive = isActive;
        account.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Accounts.UpdateAsync(account);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<decimal> GetBalanceAsync(int id)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"الحساب غير موجود");
        return account.CurrentBalance;
    }

    public async Task<IEnumerable<AccountDto>> GetSubAccountsAsync(int parentId)
    {
        var accounts = await _unitOfWork.Accounts.GetChildrenAsync(parentId);
        return _mapper.Map<IEnumerable<AccountDto>>(accounts);
    }

    public async Task<bool> IsCodeUniqueAsync(string code)
    {
        return !await _unitOfWork.Accounts.CodeExistsAsync(code);
    }

    public async Task<LedgerReportDto> GetGeneralLedgerAsync(int accountId, DateTime startDate, DateTime endDate)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(accountId)
            ?? throw new KeyNotFoundException("الحساب غير موجود");

        // 1. حساب الرصيد الافتتاحي (كل ما قبل تاريخ البداية)
        var previousLines = await _unitOfWork.JournalEntries.GetLinesByAccountIdAsync(accountId, null, startDate.AddTicks(-1));
        decimal openingBalance = previousLines.Sum(l => l.Debit - l.Credit);

        // 2. جلب حركات الفترة المطلوبة
        var currentLines = await _unitOfWork.JournalEntries.GetLinesByAccountIdAsync(accountId, startDate, endDate);

        var report = new LedgerReportDto
        {
            AccountId = account.Id,
            AccountName = account.Name,
            AccountCode = account.Code,
            StartDate = startDate,
            EndDate = endDate,
            OpeningBalance = openingBalance,
            Entries = new List<LedgerEntryDto>()
        };

        decimal runningBalance = openingBalance;
        decimal totalDebit = 0;
        decimal totalCredit = 0;

        foreach (var line in currentLines)
        {
            runningBalance += (line.Debit - line.Credit);
            totalDebit += line.Debit;
            totalCredit += line.Credit;

            report.Entries.Add(new LedgerEntryDto
            {
                Date = line.JournalEntry.EntryDate,
                VoucherNumber = line.JournalEntry.VoucherNumber,
                Description = line.Description ?? line.JournalEntry.Description,
                Debit = line.Debit,
                Credit = line.Credit,
                RunningBalance = runningBalance,
                JournalEntryId = line.JournalEntryId
            });
        }

        report.TotalDebit = totalDebit;
        report.TotalCredit = totalCredit;
        report.ClosingBalance = runningBalance;

        return report;
    }

    public async Task<TrialBalanceDto> GetTrialBalanceAsync(DateTime? asOfDate)
    {
        var allLines = await _unitOfWork.JournalEntries.GetAllLinesAsync(asOfDate);
        var accounts = await _unitOfWork.Accounts.GetAllAsync();

        var report = new TrialBalanceDto
        {
            AsOfDate = asOfDate ?? DateTime.Now,
            Items = new List<TrialBalanceItemDto>()
        };

        // تجميع الحركات حسب الحساب
        var groupedLines = allLines.GroupBy(l => l.AccountId);

        foreach (var account in accounts.Where(a => !a.IsMainAccount)) // فقط الحسابات الفرعية
        {
            var accountLines = groupedLines.FirstOrDefault(g => g.Key == account.Id);
            
            decimal debit = accountLines?.Sum(l => l.Debit) ?? 0;
            decimal credit = accountLines?.Sum(l => l.Credit) ?? 0;

            if (debit != 0 || credit != 0) // فقط الحسابات التي عليها حركات
            {
                report.Items.Add(new TrialBalanceItemDto
                {
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    Debit = debit,
                    Credit = credit
                });
            }
        }

        report.TotalDebit = report.Items.Sum(i => i.Debit);
        report.TotalCredit = report.Items.Sum(i => i.Credit);

        return report;
    }

    public async Task<IncomeStatementDto> GetIncomeStatementAsync(DateTime startDate, DateTime endDate)
    {
        // جلب حركات الفترة
        var allLines = await _unitOfWork.JournalEntries.GetAllLinesAsync(endDate);
        var periodLines = allLines.Where(l => l.JournalEntry.EntryDate >= startDate);

        var report = new IncomeStatementDto
        {
            FromDate = startDate,
            ToDate = endDate
        };

        // 1. الإيرادات (تبدأ بـ 4)
        var revenues = periodLines.Where(l => l.Account.Code.StartsWith("4"));
        var groupedRevenues = revenues.GroupBy(l => new { l.Account.Code, l.Account.Name });
        
        foreach (var group in groupedRevenues)
        {
            // في الإيرادات، الزيادة تكون في الطرف الدائن
            decimal amount = group.Sum(l => l.Credit - l.Debit);
            if (amount != 0)
            {
                report.RevenueDetails.Add(new FinancialReportItemDto
                {
                    AccountCode = group.Key.Code,
                    AccountName = group.Key.Name,
                    Amount = amount
                });
            }
        }
        report.TotalRevenue = report.RevenueDetails.Sum(r => r.Amount);

        // 2. المصروفات (تبدأ بـ 5)
        var expenses = periodLines.Where(l => l.Account.Code.StartsWith("5"));
        var groupedExpenses = expenses.GroupBy(l => new { l.Account.Code, l.Account.Name });

        foreach (var group in groupedExpenses)
        {
            // في المصروفات، الزيادة تكون في الطرف المدين
            decimal amount = group.Sum(l => l.Debit - l.Credit);
            if (amount != 0)
            {
                report.ExpenseDetails.Add(new FinancialReportItemDto
                {
                    AccountCode = group.Key.Code,
                    AccountName = group.Key.Name,
                    Amount = amount
                });
            }
        }
        report.TotalExpenses = report.ExpenseDetails.Sum(e => e.Amount);

        return report;
    }

    public async Task<BalanceSheetDto> GetBalanceSheetAsync(DateTime asOfDate)
    {
        var trialBalance = await GetTrialBalanceAsync(asOfDate);
        
        var report = new BalanceSheetDto
        {
            AsOfDate = asOfDate
        };

        // 1. الأصول (تبدأ بـ 1)
        report.AssetDetails = trialBalance.Items
            .Where(i => i.AccountCode.StartsWith("1"))
            .Select(i => new FinancialReportItemDto { AccountCode = i.AccountCode, AccountName = i.AccountName, Amount = i.Balance })
            .ToList();
        report.TotalAssets = report.AssetDetails.Sum(a => a.Amount);

        // 2. الخصوم (تبدأ بـ 2)
        report.LiabilityDetails = trialBalance.Items
            .Where(i => i.AccountCode.StartsWith("2"))
            .Select(i => new FinancialReportItemDto { AccountCode = i.AccountCode, AccountName = i.AccountName, Amount = -i.Balance }) // عكس الإشارة لأن رصيدها دائن
            .ToList();
        report.TotalLiabilities = report.LiabilityDetails.Sum(l => l.Amount);

        // 3. حقوق الملكية (تبدأ بـ 3)
        report.EquityDetails = trialBalance.Items
            .Where(i => i.AccountCode.StartsWith("3"))
            .Select(i => new FinancialReportItemDto { AccountCode = i.AccountCode, AccountName = i.AccountName, Amount = -i.Balance })
            .ToList();

        // حساب صافي ربح الفترة الحالية لدمجه في حقوق الملكية
        // ملاحظة: حساب الربح هنا هو الفرق بين الأصول والخصوم/حقوق الملكية في الميزانية (أو نتيجة قائمة الدخل)
        var incomeStatement = await GetIncomeStatementAsync(new DateTime(asOfDate.Year, 1, 1), asOfDate);
        report.NetCurrentYearProfit = incomeStatement.NetProfit;
        
        report.TotalEquity = report.EquityDetails.Sum(e => e.Amount) + report.NetCurrentYearProfit;

        return report;
    }

    public async Task<IEnumerable<LedgerReportDto>> GetAllLedgersAsync(DateTime startDate, DateTime endDate)
    {
        var accounts = await _unitOfWork.Accounts.GetAllAsync();
        var allLines = await _unitOfWork.JournalEntries.GetAllLinesAsync(endDate);
        
        // جلب جميع الحركات السابقة للرصيد الافتتاحي دفعة واحدة لتحسين الأداء
        var openingLines = await _unitOfWork.JournalEntries.GetAllLinesAsync(startDate.AddTicks(-1));
        var openingBalances = openingLines
            .GroupBy(l => l.AccountId)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Debit - l.Credit));

        var result = new List<LedgerReportDto>();

        foreach (var account in accounts.Where(a => !a.IsMainAccount))
        {
            var currentLines = allLines
                .Where(l => l.AccountId == account.Id && l.JournalEntry.EntryDate >= startDate)
                .OrderBy(l => l.JournalEntry.EntryDate)
                .ToList();

            if (!currentLines.Any()) continue; // تخطي الحسابات التي ليس لها حركات في الفترة

            decimal openingBalance = openingBalances.ContainsKey(account.Id) ? openingBalances[account.Id] : 0;
            decimal runningBalance = openingBalance;
            
            var entries = new List<LedgerEntryDto>();
            decimal totalDebit = 0;
            decimal totalCredit = 0;

            foreach (var line in currentLines)
            {
                runningBalance += (line.Debit - line.Credit);
                totalDebit += line.Debit;
                totalCredit += line.Credit;

                entries.Add(new LedgerEntryDto
                {
                    Date = line.JournalEntry.EntryDate,
                    VoucherNumber = line.JournalEntry.VoucherNumber,
                    Description = line.Description ?? line.JournalEntry.Description,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    RunningBalance = runningBalance,
                    JournalEntryId = line.JournalEntryId
                });
            }

            result.Add(new LedgerReportDto
            {
                AccountId = account.Id,
                AccountName = account.Name,
                AccountCode = account.Code,
                StartDate = startDate,
                EndDate = endDate,
                OpeningBalance = openingBalance,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                ClosingBalance = runningBalance,
                Entries = entries
            });
        }

        return result.OrderBy(r => r.AccountCode);
    }
}
