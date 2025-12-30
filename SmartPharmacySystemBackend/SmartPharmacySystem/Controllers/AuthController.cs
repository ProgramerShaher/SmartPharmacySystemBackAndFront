using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Auth;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

/// <summary>
/// Authentication controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUserService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// تسجيل الدخول
    /// Login
    /// </summary>
    /// <access>Everyone (Anonymous)</access>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(ApiResponse<LoginResponseDto>.Succeeded(response, "تم تسجيل الدخول بنجاح"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed: {Message}", ex.Message);
            return Unauthorized(ApiResponse<LoginResponseDto>.Failed(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, ApiResponse<LoginResponseDto>.Failed("حدث خطأ أثناء تسجيل الدخول"));
        }
    }

    /// <summary>
    /// تغيير كلمة المرور
    /// Change password
    /// </summary>
    /// <access>Admin | Pharmacist</access>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.Failed("المستخدم غير مصادق عليه"));
            }

            await _authService.ChangePasswordAsync(userId.Value, request);
            return Ok(ApiResponse<object>.Succeeded(null, "تم تغيير كلمة المرور بنجاح"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Password change failed: {Message}", ex.Message);
            return BadRequest(ApiResponse<object>.Failed(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change");
            return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء تغيير كلمة المرور"));
        }
    }

    /// <summary>
    /// الحصول على معلومات المستخدم الحالي
    /// Get current user information
    /// </summary>
    /// <access>Admin | Pharmacist</access>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        try
        {
            var currentUser = new
            {
                UserId = _currentUserService.UserId,
                Username = _currentUserService.Username,
                Role = _currentUserService.Role,
                RoleId = _currentUserService.RoleId,
                IsAdmin = _currentUserService.IsAdmin,
                IsPharmacist = _currentUserService.IsPharmacist
            };

            return Ok(ApiResponse<object>.Succeeded(currentUser, "تم جلب معلومات المستخدم بنجاح"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء جلب معلومات المستخدم"));
        }
    }
}
