using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartPharmacySystem.Application.DTOs.Auth;
using SmartPharmacySystem.Application.IServices;
using SmartPharmacySystem.Application.Interfaces.Data;
using SmartPharmacySystem.Core.Entities;

namespace SmartPharmacySystem.Application.Services;

/// <summary>
/// Mobile customer authentication service.
/// Handles registration and login for pharmacy app customers.
/// </summary>
public class MobileAuthService : IMobileAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public MobileAuthService(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<MobileAuthResponseDto> RegisterAsync(MobileRegisterDto dto)
    {
        // Check if phone number already registered
        var existing = await _context.Customers
            .FirstOrDefaultAsync(c => c.PhoneNumber == dto.PhoneNumber && !c.IsDeleted);

        if (existing != null)
            throw new InvalidOperationException("رقم الهاتف مسجل مسبقاً. يرجى تسجيل الدخول.");

        var customer = new Customer
        {
            Name = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var token = GenerateToken(customer);
        return new MobileAuthResponseDto
        {
            CustomerId = customer.Id,
            FullName = customer.Name,
            PhoneNumber = customer.PhoneNumber!,
            Token = token
        };
    }

    public async Task<MobileAuthResponseDto> LoginAsync(MobileLoginDto dto)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.PhoneNumber == dto.PhoneNumber && !c.IsDeleted);

        if (customer == null || string.IsNullOrEmpty(customer.PasswordHash))
            throw new UnauthorizedAccessException("رقم الهاتف أو كلمة المرور غير صحيحة");

        if (!customer.IsActive)
            throw new UnauthorizedAccessException("الحساب موقوف. يرجى التواصل مع الصيدلية");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, customer.PasswordHash))
            throw new UnauthorizedAccessException("رقم الهاتف أو كلمة المرور غير صحيحة");

        var token = GenerateToken(customer);
        return new MobileAuthResponseDto
        {
            CustomerId = customer.Id,
            FullName = customer.Name,
            PhoneNumber = customer.PhoneNumber!,
            Token = token
        };
    }

    private string GenerateToken(Customer customer)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
            new Claim(ClaimTypes.Name, customer.Name),
            new Claim(ClaimTypes.MobilePhone, customer.PhoneNumber ?? ""),
            new Claim("CustomerId", customer.Id.ToString()),
            new Claim(ClaimTypes.Role, "Customer")
        };

        var expirationDays = 30; // Mobile tokens last 30 days
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(expirationDays),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
