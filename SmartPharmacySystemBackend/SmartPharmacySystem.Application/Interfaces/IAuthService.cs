using SmartPharmacySystem.Application.DTOs.Auth;

namespace SmartPharmacySystem.Application.Interfaces;

/// <summary>
/// واجهة خدمة المصادقة
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// تسجيل الدخول
    /// Login user and generate JWT token
    /// </summary>
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// تغيير كلمة المرور
    /// Change user password
    /// </summary>
    Task ChangePasswordAsync(int userId, ChangePasswordDto request);

    /// <summary>
    /// التحقق من صحة كلمة المرور
    /// Verify password against hash
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);

    /// <summary>
    /// تشفير كلمة المرور
    /// Hash password using BCrypt
    /// </summary>
    string HashPassword(string password);
}
