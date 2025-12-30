namespace SmartPharmacySystem.Application.DTOs.User;

/// <summary>
/// كائن نقل البيانات لإنشاء مستخدم جديد.
/// يحتوي على البيانات المطلوبة لإنشاء حساب مستخدم.
/// </summary>
public class CreateUserDto
{
    /// <summary>
    /// اسم المستخدم (اسم الدخول)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// كلمة المرور
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    /// <summary>
    /// إعادة كلمة المرور
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// الاسم الكامل للمستخدم
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// البريد الإلكتروني
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// الدور (e.g., Admin, Pharmacist, Cashier)
    /// </summary>
    public int RoleId { get; set; }
    /// <summary>
    /// رقم الهاتف
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// ملاحظات إضافية
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}
