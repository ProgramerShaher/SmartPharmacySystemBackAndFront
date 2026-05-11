using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacySystem.Application.DTOs.Auth;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.Wrappers;

namespace SmartPharmacySystem.Controllers;

/// <summary>
/// Mobile App Authentication Controller
/// Handles customer registration and login for the pharmacy mobile app.
/// </summary>
[ApiController]
[Route("api/mobile/auth")]
public class MobileAuthController : ControllerBase
{
    private readonly IMobileAuthService _mobileAuthService;
    private readonly ILogger<MobileAuthController> _logger;

    public MobileAuthController(IMobileAuthService mobileAuthService, ILogger<MobileAuthController> logger)
    {
        _mobileAuthService = mobileAuthService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new customer account in the mobile app.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] MobileRegisterDto dto)
    {
        try
        {
            var result = await _mobileAuthService.RegisterAsync(dto);
            return Ok(ApiResponse<MobileAuthResponseDto>.Succeeded(result, "تم إنشاء الحساب بنجاح"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Failed(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during mobile registration");
            return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء إنشاء الحساب"));
        }
    }

    /// <summary>
    /// Login with phone number and password.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] MobileLoginDto dto)
    {
        try
        {
            var result = await _mobileAuthService.LoginAsync(dto);
            return Ok(ApiResponse<MobileAuthResponseDto>.Succeeded(result, "تم تسجيل الدخول بنجاح"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.Failed(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during mobile login");
            return StatusCode(500, ApiResponse<object>.Failed("حدث خطأ أثناء تسجيل الدخول"));
        }
    }
}
