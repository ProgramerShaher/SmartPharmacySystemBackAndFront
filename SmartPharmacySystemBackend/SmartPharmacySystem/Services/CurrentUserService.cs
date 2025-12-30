using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SmartPharmacySystem.Application.Interfaces;

namespace SmartPharmacySystem.Services;

/// <summary>
/// خدمة المستخدم الحالي
/// Current user service implementation
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;

            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    public int? RoleId
    {
        get
        {
            var roleIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("RoleId")?.Value;
            return int.TryParse(roleIdClaim, out var roleId) ? roleId : null;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;

    public bool IsPharmacist => Role?.Equals("Pharmacist", StringComparison.OrdinalIgnoreCase) ?? false;
}
