namespace SmartPharmacySystem.Core.Enums;

/// <summary>
/// حالة المستخدم في النظام
/// User status in the system
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// نشط - يمكن للمستخدم تسجيل الدخول والعمل
    /// Active - User can login and work
    /// </summary>
    Active = 1,

    /// <summary>
    /// غير نشط - لا يمكن للمستخدم تسجيل الدخول
    /// Inactive - User cannot login
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// موقوف - تم إيقاف الحساب مؤقتاً
    /// Suspended - Account temporarily suspended
    /// </summary>
    Suspended = 3
}
