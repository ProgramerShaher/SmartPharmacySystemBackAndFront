using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Infrastructure.Data.Seeders;

public static class ChartOfAccountsSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // تحقق مما إذا كانت هناك أي حسابات بالفعل
        if (await context.Accounts.AnyAsync())
        {
            return; // الشجرة موجودة مسبقاً
        }

        // 1. الأصول (Assets)
        var assets = new Account { Code = "1", Name = "الأصول", Type = AccountType.Asset, IsMainAccount = true, IsActive = true };
        context.Accounts.Add(assets);
        await context.SaveChangesAsync();

        var currentAssets = new Account { Code = "11", Name = "الأصول المتداولة", Type = AccountType.Asset, IsMainAccount = true, ParentId = assets.Id, IsActive = true };
        var fixedAssets = new Account { Code = "12", Name = "الأصول الثابتة", Type = AccountType.Asset, IsMainAccount = true, ParentId = assets.Id, IsActive = true };
        context.Accounts.AddRange(currentAssets, fixedAssets);
        await context.SaveChangesAsync();

        // حسابات الأصول المتداولة
        var cashAndBanks = new Account { Code = "111", Name = "النقدية والبنوك", Type = AccountType.Asset, IsMainAccount = true, ParentId = currentAssets.Id, IsActive = true };
        var receivables = new Account { Code = "112", Name = "العملاء والذمم المدينة", Type = AccountType.Asset, IsMainAccount = true, ParentId = currentAssets.Id, IsActive = true };
        var inventory = new Account { Code = "113", Name = "المخزون", Type = AccountType.Asset, IsMainAccount = true, ParentId = currentAssets.Id, IsActive = true };
        context.Accounts.AddRange(cashAndBanks, receivables, inventory);
        await context.SaveChangesAsync();

        // حسابات النقدية (فرعية)
        context.Accounts.Add(new Account { Code = "11101", Name = "الصندوق الرئيسي", Type = AccountType.Asset, IsMainAccount = false, ParentId = cashAndBanks.Id, IsActive = true });
        context.Accounts.Add(new Account { Code = "11102", Name = "بنك (حساب جاري)", Type = AccountType.Asset, IsMainAccount = false, ParentId = cashAndBanks.Id, IsActive = true });
        
        // حسابات المخزون (فرعية)
        context.Accounts.Add(new Account { Code = "11301", Name = "مخزون الأدوية", Type = AccountType.Asset, IsMainAccount = false, ParentId = inventory.Id, IsActive = true });
        context.Accounts.Add(new Account { Code = "11302", Name = "مخزون مستلزمات طبية", Type = AccountType.Asset, IsMainAccount = false, ParentId = inventory.Id, IsActive = true });

        // حسابات الأصول الثابتة (فرعية)
        context.Accounts.Add(new Account { Code = "12001", Name = "أثاث وتجهيزات طبية", Type = AccountType.Asset, IsMainAccount = false, ParentId = fixedAssets.Id, IsActive = true });
        context.Accounts.Add(new Account { Code = "12002", Name = "أجهزة كمبيوتر ومعدات", Type = AccountType.Asset, IsMainAccount = false, ParentId = fixedAssets.Id, IsActive = true });

        // 2. الخصوم (Liabilities)
        var liabilities = new Account { Code = "2", Name = "الخصوم", Type = AccountType.Liability, IsMainAccount = true, IsActive = true };
        context.Accounts.Add(liabilities);
        await context.SaveChangesAsync();

        var currentLiabilities = new Account { Code = "21", Name = "الخصوم المتداولة", Type = AccountType.Liability, IsMainAccount = true, ParentId = liabilities.Id, IsActive = true };
        context.Accounts.Add(currentLiabilities);
        await context.SaveChangesAsync();

        var payables = new Account { Code = "211", Name = "الموردين والذمم الدائنة", Type = AccountType.Liability, IsMainAccount = true, ParentId = currentLiabilities.Id, IsActive = true };
        context.Accounts.Add(payables);
        await context.SaveChangesAsync();

        // حسابات الموردين (فرعية)
        context.Accounts.Add(new Account { Code = "21101", Name = "ذمم الموردين", Type = AccountType.Liability, IsMainAccount = false, ParentId = payables.Id, IsActive = true });
        context.Accounts.Add(new Account { Code = "21102", Name = "أوراق الدفع", Type = AccountType.Liability, IsMainAccount = false, ParentId = payables.Id, IsActive = true });

        // 3. حقوق الملكية (Equity)
        var equity = new Account { Code = "3", Name = "حقوق الملكية", Type = AccountType.Equity, IsMainAccount = true, IsActive = true };
        context.Accounts.Add(equity);
        await context.SaveChangesAsync();

        var capital = new Account { Code = "31", Name = "رأس المال", Type = AccountType.Equity, IsMainAccount = true, ParentId = equity.Id, IsActive = true };
        var retainedEarnings = new Account { Code = "32", Name = "الأرباح المحتجزة", Type = AccountType.Equity, IsMainAccount = true, ParentId = equity.Id, IsActive = true };
        context.Accounts.AddRange(capital, retainedEarnings);
        await context.SaveChangesAsync();

        // حسابات حقوق الملكية (فرعية)
        context.Accounts.Add(new Account { Code = "31001", Name = "رأس مال الشركاء", Type = AccountType.Equity, IsMainAccount = false, ParentId = capital.Id, IsActive = true });
        context.Accounts.Add(new Account { Code = "32001", Name = "أرباح العام الحالي", Type = AccountType.Equity, IsMainAccount = false, ParentId = retainedEarnings.Id, IsActive = true });

        // 4. الإيرادات (Revenues)
        var revenues = new Account { Code = "4", Name = "الإيرادات", Type = AccountType.Revenue, IsMainAccount = true, IsActive = true };
        context.Accounts.Add(revenues);
        await context.SaveChangesAsync();

        var operatingRevenues = new Account { Code = "41", Name = "الإيرادات التشغيلية", Type = AccountType.Revenue, IsMainAccount = true, ParentId = revenues.Id, IsActive = true };
        var otherRevenues = new Account { Code = "42", Name = "إيرادات أخرى", Type = AccountType.Revenue, IsMainAccount = true, ParentId = revenues.Id, IsActive = true };
        context.Accounts.AddRange(operatingRevenues, otherRevenues);
        await context.SaveChangesAsync();

        // حسابات الإيرادات (فرعية)
        context.Accounts.Add(new Account { Code = "41001", Name = "إيرادات مبيعات الأدوية", Type = AccountType.Revenue, IsMainAccount = false, ParentId = operatingRevenues.Id, IsActive = true });
        context.Accounts.Add(new Account { Code = "41002", Name = "إيرادات مبيعات المستلزمات", Type = AccountType.Revenue, IsMainAccount = false, ParentId = operatingRevenues.Id, IsActive = true });
        context.Accounts.Add(new Account { Code = "42001", Name = "خصم مكتسب", Type = AccountType.Revenue, IsMainAccount = false, ParentId = otherRevenues.Id, IsActive = true });

        // 5. المصروفات (Expenses)
        var expenses = new Account { Code = "5", Name = "المصروفات", Type = AccountType.Expense, IsMainAccount = true, IsActive = true };
        context.Accounts.Add(expenses);
        await context.SaveChangesAsync();

        var cogs = new Account { Code = "51", Name = "تكلفة البضاعة المباعة", Type = AccountType.Expense, IsMainAccount = true, ParentId = expenses.Id, IsActive = true };
        var operatingExpenses = new Account { Code = "52", Name = "المصروفات التشغيلية", Type = AccountType.Expense, IsMainAccount = true, ParentId = expenses.Id, IsActive = true };
        context.Accounts.AddRange(cogs, operatingExpenses);
        await context.SaveChangesAsync();

        // حسابات المصروفات (فرعية)
        context.Accounts.Add(new Account { Code = "51001", Name = "تكلفة الأدوية المباعة", Type = AccountType.Expense, IsMainAccount = false, ParentId = cogs.Id, IsActive = true });
        context.Accounts.Add(new Account { Code = "52001", Name = "الرواتب والأجور", Type = AccountType.Expense, IsMainAccount = false, ParentId = operatingExpenses.Id, IsActive = true });
        context.Accounts.Add(new Account { Code = "52002", Name = "مصاريف الإيجار", Type = AccountType.Expense, IsMainAccount = false, ParentId = operatingExpenses.Id, IsActive = true });
        context.Accounts.Add(new Account { Code = "52003", Name = "مصاريف الكهرباء والمياه", Type = AccountType.Expense, IsMainAccount = false, ParentId = operatingExpenses.Id, IsActive = true });
        context.Accounts.Add(new Account { Code = "52004", Name = "رسوم بنكية", Type = AccountType.Expense, IsMainAccount = false, ParentId = operatingExpenses.Id, IsActive = true });
        context.Accounts.Add(new Account { Code = "52005", Name = "خصم مسموح به", Type = AccountType.Expense, IsMainAccount = false, ParentId = operatingExpenses.Id, IsActive = true });

        await context.SaveChangesAsync();
    }
}
