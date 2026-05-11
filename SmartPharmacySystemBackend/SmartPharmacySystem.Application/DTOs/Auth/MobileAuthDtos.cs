namespace SmartPharmacySystem.Application.DTOs.Auth;

public class MobileRegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public class MobileLoginDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class MobileAuthResponseDto
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
