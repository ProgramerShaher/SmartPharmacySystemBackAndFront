namespace SmartPharmacySystem.Application.DTOs.Auth;

/// <summary>
/// استجابة تسجيل الدخول
/// Login response DTO
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// رمز JWT
    /// JWT Token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// معرف المستخدم
    /// User ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// اسم المستخدم
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// الاسم الكامل
    /// Full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// اسم الدور
    /// Role name
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// البريد الإلكتروني
    /// Email
    /// </summary>
    public string? Email { get; set; }
}
