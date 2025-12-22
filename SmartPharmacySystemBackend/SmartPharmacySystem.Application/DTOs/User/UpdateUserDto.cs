namespace SmartPharmacySystem.Application.DTOs.User;

/// <summary>
/// كائن نقل البيانات لتحديث بيانات مستخدم.
/// يحتوي على البيانات القابلة للتحديث للمستخدم.
/// </summary>
public class UpdateUserDto
{
    /// <summary>
    /// معرف المستخدم
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// اسم المستخدم (اسم الدخول)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// كلمة المرور
    /// </summary>
    public string Password { get; set; } = string.Empty;

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
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// رقم الهاتف
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// ملاحظات إضافية
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}
