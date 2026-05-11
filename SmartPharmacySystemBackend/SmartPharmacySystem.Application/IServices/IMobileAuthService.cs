using SmartPharmacySystem.Application.DTOs.Auth;

namespace SmartPharmacySystem.Application.IServices;

public interface IMobileAuthService
{
    Task<MobileAuthResponseDto> RegisterAsync(MobileRegisterDto dto);
    Task<MobileAuthResponseDto> LoginAsync(MobileLoginDto dto);
}
