namespace SmartPharmacySystem.Application.DTOs.Auth;

/// <summary>
/// طلب تسجيل الدخول
/// Login request DTO
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// اسم المستخدم
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// كلمة المرور
    /// Password
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
