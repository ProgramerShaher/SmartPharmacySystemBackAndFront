using SmartPharmacySystem.Core.Entities;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Application.Interfaces;

/// <summary>
/// خدمة استعلام وبحث ديناميكية مركزية عن الحسابات المحاسبية بدليل الحسابات (Chart of Accounts).
/// تضمن استخراج المعرف الحقيقي (account.Id) بالاعتماد على الأكواد المعتمدة والبحث الدلالي
/// لمنع أي استخدام لـ Hardcoded Account IDs في النظام.
/// </summary>
public interface IAccountLookupService
{
    /// <summary>
    /// استرجاع حساب الصندوق / النقدية (يدعم صندوق المستخدم المخصص إذا وُجد)
    /// </summary>
    Task<Account> GetCashAccountAsync(int? userId = null);

    /// <summary>
    /// استرجاع حساب البنك / الدفع بالشبكة والبطاقات
    /// </summary>
    Task<Account> GetCardAccountAsync();

    /// <summary>
    /// استرجاع حساب العملاء والذمم المدينة
    /// </summary>
    Task<Account> GetReceivablesAccountAsync();

    /// <summary>
    /// استرجاع حساب الموردين والذمم الدائنة
    /// </summary>
    Task<Account> GetPayablesAccountAsync();

    /// <summary>
    /// استرجاع حساب مخزون البضاعة / الأدوية
    /// </summary>
    Task<Account> GetInventoryAccountAsync();

    /// <summary>
    /// استرجاع حساب إيرادات المبيعات
    /// </summary>
    Task<Account> GetSalesRevenueAccountAsync();

    /// <summary>
    /// استرجاع حساب تكلفة البضاعة المباعة (COGS)
    /// </summary>
    Task<Account> GetCostOfGoodsSoldAccountAsync();

    /// <summary>
    /// استرجاع حساب خسائر البضاعة التالفة
    /// </summary>
    Task<Account> GetDamagedGoodsExpenseAccountAsync();

    /// <summary>
    /// استرجاع حساب مصروف الرواتب والأجور
    /// </summary>
    Task<Account> GetSalaryExpenseAccountAsync();

    /// <summary>
    /// استرجاع حساب الخصم المسموح به
    /// </summary>
    Task<Account> GetDiscountAccountAsync();

    /// <summary>
    /// استرجاع حساب ضريبة القيمة المضافة المستحقة (Output VAT)
    /// </summary>
    Task<Account> GetVatPayableAccountAsync();

    /// <summary>
    /// استرجاع حساب المصروفات التشغيلية العامة الافتراضي
    /// </summary>
    Task<Account> GetOperatingExpenseAccountAsync();
}
