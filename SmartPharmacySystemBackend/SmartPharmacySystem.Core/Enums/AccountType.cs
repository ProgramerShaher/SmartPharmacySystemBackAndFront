namespace SmartPharmacySystem.Core.Enums;

/// <summary>
/// أنواع الحسابات في شجرة الحسابات
/// </summary>
public enum AccountType
{
    /// <summary>
    /// الأصول (مثل الصندوق، المخزون، البنك)
    /// </summary>
    Asset = 1,

    /// <summary>
    /// الخصوم / الالتزامات (مثل مديونيات الموردين)
    /// </summary>
    Liability = 2,

    /// <summary>
    /// حقوق الملكية (رأس المال، الأرباح)
    /// </summary>
    Equity = 3,

    /// <summary>
    /// الإيرادات (المبيعات)
    /// </summary>
    Revenue = 4,

    /// <summary>
    /// المصروفات (إيجار، رواتب، كهرباء)
    /// </summary>
    Expense = 5
}
