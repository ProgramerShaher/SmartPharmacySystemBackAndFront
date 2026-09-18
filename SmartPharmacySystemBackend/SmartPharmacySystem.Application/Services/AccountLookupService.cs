using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

/// <summary>
/// تطبيق خدمة استعلام وبحث ديناميكية مركزية عن الحسابات المحاسبية بدليل الحسابات (Chart of Accounts).
/// تضمن استخراج المعرف الحقيقي (account.Id) بالاعتماد على الأكواد المعتمدة والبحث الدلالي
/// لمنع أي استخدام لـ Hardcoded Account IDs في النظام.
/// </summary>
public class AccountLookupService : IAccountLookupService
{
    private readonly IUnitOfWork _unitOfWork;

    public AccountLookupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Account> GetCashAccountAsync(int? userId = null)
    {
        if (userId.HasValue)
        {
            var userCash = await _unitOfWork.Accounts.GetByCodeAsync($"11101-{userId.Value}");
            if (userCash != null && userCash.IsActive) return userCash;
        }

        var cash = await _unitOfWork.Accounts.GetByCodeAsync("11101")
                   ?? await _unitOfWork.Accounts.GetByCodeAsync("1101")
                   ?? await _unitOfWork.Accounts.GetByCodeAsync("111");

        if (cash != null && cash.IsActive && !cash.IsMainAccount) return cash;

        var allAccounts = await _unitOfWork.Accounts.GetAllAsync();
        var candidate = allAccounts.FirstOrDefault(a => a.IsActive && !a.IsMainAccount && (a.Name.Contains("صندوق") || a.Name.Contains("نقد")))
                        ?? cash
                        ?? allAccounts.FirstOrDefault(a => a.IsActive && a.Type == AccountType.Asset && !a.IsMainAccount)
                        ?? allAccounts.FirstOrDefault(a => a.IsActive && a.Type == AccountType.Asset)
                        ?? throw new InvalidOperationException("حساب الصندوق / النقدية غير موجود في دليل الحسابات. يرجى تهيئة دليل الحسابات أولاً.");

        return candidate;
    }

    public async Task<Account> GetCardAccountAsync()
    {
        var card = await _unitOfWork.Accounts.GetByCodeAsync("11102");
        if (card != null && card.IsActive && !card.IsMainAccount) return card;

        var allAccounts = await _unitOfWork.Accounts.GetAllAsync();
        var candidate = allAccounts.FirstOrDefault(a => a.IsActive && !a.IsMainAccount && (a.Name.Contains("بنك") || a.Name.Contains("شبكة") || a.Name.Contains("بطاقة") || a.Name.Contains("مدى")))
                        ?? card
                        ?? await GetCashAccountAsync();

        return candidate;
    }

    public async Task<Account> GetReceivablesAccountAsync()
    {
        var recv = await _unitOfWork.Accounts.GetByCodeAsync("11201")
                   ?? await _unitOfWork.Accounts.GetByCodeAsync("112");

        if (recv != null && recv.IsActive && !recv.IsMainAccount) return recv;

        var allAccounts = await _unitOfWork.Accounts.GetAllAsync();
        var candidate = allAccounts.FirstOrDefault(a => a.IsActive && !a.IsMainAccount && (a.Name.Contains("عملاء") || a.Name.Contains("ذمم مدينة") || a.Name.Contains("المدينون")))
                        ?? recv
                        ?? allAccounts.FirstOrDefault(a => a.IsActive && a.Type == AccountType.Asset && (a.Name.Contains("عملاء") || a.Name.Contains("ذمم")))
                        ?? throw new InvalidOperationException("حساب العملاء والذمم المدينة غير موجود في دليل الحسابات.");

        return candidate;
    }

    public async Task<Account> GetPayablesAccountAsync()
    {
        var pay = await _unitOfWork.Accounts.GetByCodeAsync("21101")
                  ?? await _unitOfWork.Accounts.GetByCodeAsync("211")
                  ?? await _unitOfWork.Accounts.GetByCodeAsync("2101");

        if (pay != null && pay.IsActive && !pay.IsMainAccount) return pay;

        var allAccounts = await _unitOfWork.Accounts.GetAllAsync();
        var candidate = allAccounts.FirstOrDefault(a => a.IsActive && !a.IsMainAccount && (a.Name.Contains("موردين") || a.Name.Contains("ذمم دائنة") || a.Name.Contains("الدائنون")))
                        ?? pay
                        ?? allAccounts.FirstOrDefault(a => a.IsActive && a.Type == AccountType.Liability && (a.Name.Contains("موردين") || a.Name.Contains("ذمم")))
                        ?? throw new InvalidOperationException("حساب الموردين والذمم الدائنة غير موجود في دليل الحسابات.");

        return candidate;
    }

    public async Task<Account> GetInventoryAccountAsync()
    {
        var inv = await _unitOfWork.Accounts.GetByCodeAsync("11301")
                  ?? await _unitOfWork.Accounts.GetByCodeAsync("113")
                  ?? await _unitOfWork.Accounts.GetByCodeAsync("1301");

        if (inv != null && inv.IsActive && !inv.IsMainAccount) return inv;

        var allAccounts = await _unitOfWork.Accounts.GetAllAsync();
        var candidate = allAccounts.FirstOrDefault(a => a.IsActive && !a.IsMainAccount && (a.Name.Contains("مخزون") || a.Name.Contains("بضاعة")))
                        ?? inv
                        ?? allAccounts.FirstOrDefault(a => a.IsActive && a.Type == AccountType.Asset && a.Name.Contains("مخزون"))
                        ?? throw new InvalidOperationException("حساب مخزون البضاعة غير موجود في دليل الحسابات.");

        return candidate;
    }

    public async Task<Account> GetSalesRevenueAccountAsync()
    {
        var sales = await _unitOfWork.Accounts.GetByCodeAsync("41001")
                    ?? await _unitOfWork.Accounts.GetByCodeAsync("41");

        if (sales != null && sales.IsActive && !sales.IsMainAccount) return sales;

        var allAccounts = await _unitOfWork.Accounts.GetAllAsync();
        var candidate = allAccounts.FirstOrDefault(a => a.IsActive && !a.IsMainAccount && (a.Name.Contains("مبيعات") || a.Name.Contains("إيرادات مبيعات")))
                        ?? sales
                        ?? allAccounts.FirstOrDefault(a => a.IsActive && a.Type == AccountType.Revenue)
                        ?? throw new InvalidOperationException("حساب إيرادات المبيعات غير موجود في دليل الحسابات.");

        return candidate;
    }

    public async Task<Account> GetCostOfGoodsSoldAccountAsync()
    {
        var cogs = await _unitOfWork.Accounts.GetByCodeAsync("51001")
                   ?? await _unitOfWork.Accounts.GetByCodeAsync("51");

        if (cogs != null && cogs.IsActive && !cogs.IsMainAccount) return cogs;

        var allAccounts = await _unitOfWork.Accounts.GetAllAsync();
        var candidate = allAccounts.FirstOrDefault(a => a.IsActive && !a.IsMainAccount && (a.Name.Contains("تكلفة") || a.Name.Contains("تكاليف")))
                        ?? cogs
                        ?? allAccounts.FirstOrDefault(a => a.IsActive && a.Type == AccountType.Expense && a.Name.Contains("تكلفة"))
                        ?? throw new InvalidOperationException("حساب تكلفة البضاعة المباعة غير موجود في دليل الحسابات.");

        return candidate;
    }

    public async Task<Account> GetDamagedGoodsExpenseAccountAsync()
    {
        var damaged = await _unitOfWork.Accounts.GetByCodeAsync("52006")
                      ?? await _unitOfWork.Accounts.GetByCodeAsync("5206");

        if (damaged != null && damaged.IsActive && !damaged.IsMainAccount) return damaged;

        var allAccounts = await _unitOfWork.Accounts.GetAllAsync();
        var candidate = allAccounts.FirstOrDefault(a => a.IsActive && !a.IsMainAccount && (a.Name.Contains("تالف") || a.Name.Contains("فاقد") || a.Name.Contains("خسائر")))
                        ?? damaged
                        ?? allAccounts.FirstOrDefault(a => a.IsActive && a.Type == AccountType.Expense && !a.IsMainAccount)
                        ?? await GetCostOfGoodsSoldAccountAsync();

        return candidate;
    }

    public async Task<Account> GetSalaryExpenseAccountAsync()
    {
        var salary = await _unitOfWork.Accounts.GetByCodeAsync("52001")
                     ?? await _unitOfWork.Accounts.GetByCodeAsync("5201");

        if (salary != null && salary.IsActive && !salary.IsMainAccount) return salary;

        var allAccounts = await _unitOfWork.Accounts.GetAllAsync();
        var candidate = allAccounts.FirstOrDefault(a => a.IsActive && !a.IsMainAccount && (a.Name.Contains("رواتب") || a.Name.Contains("أجور")))
                        ?? salary
                        ?? allAccounts.FirstOrDefault(a => a.IsActive && a.Type == AccountType.Expense && !a.IsMainAccount)
                        ?? throw new InvalidOperationException("حساب مصروف الرواتب والأجور غير موجود في دليل الحسابات.");

        return candidate;
    }

    public async Task<Account> GetDiscountAccountAsync()
    {
        var discount = await _unitOfWork.Accounts.GetByCodeAsync("52005")
                       ?? await _unitOfWork.Accounts.GetByCodeAsync("41002");

        if (discount != null && discount.IsActive && !discount.IsMainAccount) return discount;

        var allAccounts = await _unitOfWork.Accounts.GetAllAsync();
        var candidate = allAccounts.FirstOrDefault(a => a.IsActive && !a.IsMainAccount && (a.Name.Contains("خصم مسموح") || a.Name.Contains("خصومات")))
                        ?? discount
                        ?? await GetSalesRevenueAccountAsync();

        return candidate;
    }

    public async Task<Account> GetVatPayableAccountAsync()
    {
        var vat = await _unitOfWork.Accounts.GetByCodeAsync("21005")
                  ?? await _unitOfWork.Accounts.GetByCodeAsync("21105");

        if (vat != null && vat.IsActive && !vat.IsMainAccount) return vat;

        var allAccounts = await _unitOfWork.Accounts.GetAllAsync();
        var candidate = allAccounts.FirstOrDefault(a => a.IsActive && !a.IsMainAccount && (a.Name.Contains("ضريبة") || a.Name.Contains("مضافة") || a.Name.Contains("VAT")))
                        ?? vat
                        ?? allAccounts.FirstOrDefault(a => a.IsActive && a.Type == AccountType.Liability && !a.IsMainAccount)
                        ?? await GetSalesRevenueAccountAsync();

        return candidate;
    }

    public async Task<Account> GetOperatingExpenseAccountAsync()
    {
        var exp = await _unitOfWork.Accounts.GetByCodeAsync("52")
                  ?? await _unitOfWork.Accounts.GetByCodeAsync("52001");

        var allAccounts = await _unitOfWork.Accounts.GetAllAsync();
        var candidate = allAccounts.FirstOrDefault(a => a.IsActive && !a.IsMainAccount && a.Type == AccountType.Expense)
                        ?? exp
                        ?? allAccounts.FirstOrDefault(a => a.IsActive && a.Type == AccountType.Expense)
                        ?? throw new InvalidOperationException("حساب المصروفات التشغيلية غير موجود في دليل الحسابات.");

        return candidate;
    }
}
