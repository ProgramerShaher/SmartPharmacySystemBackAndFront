using SmartPharmacySystem.Application.DTOs.Financial;

namespace SmartPharmacySystem.Application.IServices;

/// <summary>
/// واجهة خدمة إدارة شجرة الحسابات
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// جلب شجرة الحسابات بالكامل بشكل هرمي
    /// </summary>
    Task<IEnumerable<AccountDto>> GetAccountTreeAsync();

    /// <summary>
    /// جلب حساب معين بواسطة المعرف
    /// </summary>
    Task<AccountDto> GetByIdAsync(int id);

    /// <summary>
    /// جلب الحسابات بناءً على النوع (مثل: أصول، خصوم، إلخ)
    /// </summary>
    Task<IEnumerable<AccountDto>> GetByAccountTypeAsync(string accountType);

    /// <summary>
    /// إضافة حساب جديد في شجرة الحسابات
    /// </summary>
    Task<AccountDto> CreateAsync(AccountDto dto);

    /// <summary>
    /// تحديث بيانات حساب موجود
    /// </summary>
    Task UpdateAsync(AccountDto dto);

    /// <summary>
    /// تحديث حالة تفعيل الحساب (نشط/غير نشط)
    /// </summary>
    Task ToggleStatusAsync(int id, bool isActive);

    /// <summary>
    /// حذف حساب من الشجرة (بشرط عدم وجود حركات مالية مرتبطة)
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// جلب الرصيد الحالي لحساب معين
    /// </summary>
    Task<decimal> GetBalanceAsync(int id);

    /// <summary>
    /// جلب الحسابات الفرعية (الأبناء) لحساب معين
    /// </summary>
    Task<IEnumerable<AccountDto>> GetSubAccountsAsync(int parentId);

    /// <summary>
    /// التحقق مما إذا كان الكود المحاسبي فريداً
    /// </summary>
    Task<bool> IsCodeUniqueAsync(string code);

    /// <summary>
    /// جلب دفتر الأستاذ (كشف حساب تفصيلي) لحساب معين
    /// </summary>
    Task<LedgerReportDto> GetGeneralLedgerAsync(int accountId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// جلب ميزان المراجعة لجميع الحسابات
    /// </summary>
    Task<TrialBalanceDto> GetTrialBalanceAsync(DateTime? asOfDate);

    /// <summary>
    /// جلب قائمة الدخل لفترة معينة
    /// </summary>
    Task<IncomeStatementDto> GetIncomeStatementAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// جلب الميزانية العمومية حتى تاريخ معين
    /// </summary>
    Task<BalanceSheetDto> GetBalanceSheetAsync(DateTime asOfDate);

    /// <summary>
    /// جلب كشف حساب تفصيلي لجميع الحسابات التي تمت عليها حركات
    /// </summary>
    Task<IEnumerable<LedgerReportDto>> GetAllLedgersAsync(DateTime startDate, DateTime endDate);
}
