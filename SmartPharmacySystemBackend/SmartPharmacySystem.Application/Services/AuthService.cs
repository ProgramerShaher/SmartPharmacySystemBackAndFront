using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SmartPharmacySystem.Application.DTOs.Auth;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using BCrypt.Net;

namespace SmartPharmacySystem.Application.Services;

/// <summary>
/// خدمة المصادقة
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// تسجيل الدخول وإنشاء JWT Token
    /// Login and generate JWT token
    /// </summary>
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        _logger.LogInformation("Login attempt for user: {Username}", request.Username);

        // البحث عن المستخدم
        var users = await _unitOfWork.Users.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Username == request.Username && !u.IsDeleted);

        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found - {Username}", request.Username);
            throw new UnauthorizedAccessException("اسم المستخدم أو كلمة المرور غير صحيحة");
        }

        // التحقق من حالة المستخدم
        if (user.Status != UserStatus.Active)
        {
            _logger.LogWarning("Login failed: User not active - {Username}", request.Username);
            throw new UnauthorizedAccessException("الحساب غير نشط. يرجى التواصل مع المدير");
        }

        // التحقق من كلمة المرور
        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password - {Username}", request.Username);
            throw new UnauthorizedAccessException("اسم المستخدم أو كلمة المرور غير صحيحة");
        }

        // تحديث آخر تسجيل دخول
        user.LastLogin = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // الحصول على معلومات الدور
        var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId);
        if (role == null)
        {
            throw new InvalidOperationException("دور المستخدم غير موجود");
        }

        // إنشاء JWT Token
        var token = GenerateJwtToken(user, role.Name);

        _logger.LogInformation("Login successful for user: {Username}", request.Username);

        return new LoginResponseDto
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            RoleName = role.Name,
            Email = user.Email
        };
    }

    /// <summary>
    /// تغيير كلمة المرور
    /// Change password
    /// </summary>
    public async Task ChangePasswordAsync(int userId, ChangePasswordDto request)
    {
        _logger.LogInformation("Password change request for user ID: {UserId}", userId);

        // التحقق من تطابق كلمة المرور الجديدة
        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new ArgumentException("كلمة المرور الجديدة وتأكيدها غير متطابقين");
        }

        // الحصول على المستخدم
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("المستخدم غير موجود");
        }

        // التحقق من كلمة المرور القديمة
        if (!VerifyPassword(request.OldPassword, user.PasswordHash))
        {
            _logger.LogWarning("Password change failed: Invalid old password - User ID: {UserId}", userId);
            throw new UnauthorizedAccessException("كلمة المرور القديمة غير صحيحة");
        }

        // تحديث كلمة المرور
        user.PasswordHash = HashPassword(request.NewPassword);
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Password changed successfully for user ID: {UserId}", userId);
    }

    /// <summary>
    /// التحقق من كلمة المرور
    /// Verify password
    /// </summary>
    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// تشفير كلمة المرور
    /// Hash password
    /// </summary>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(11));
    }

    /// <summary>
    /// إنشاء JWT Token
    /// Generate JWT token
    /// </summary>
    private string GenerateJwtToken(Core.Entities.User user, string roleName)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.GivenName, user.FullName),
            new Claim(ClaimTypes.Role, roleName),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim("UserId", user.Id.ToString()),
            new Claim("RoleId", user.RoleId.ToString())
        };

        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "480"); // Default 8 hours
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
