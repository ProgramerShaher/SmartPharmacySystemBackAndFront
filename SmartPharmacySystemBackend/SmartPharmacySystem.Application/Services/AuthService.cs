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
    private readonly IPermissionService _permissionService;

    public AuthService(
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
        _permissionService = permissionService;
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

        // سطر مؤقت للفحص
        _logger.LogInformation("Password from Request: {Pass}", request.Password);
        _logger.LogInformation("Hash from DB: {Hash}", user.PasswordHash);
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
        var tokenDetails = await GenerateJwtTokenAsync(user, role.Name, request.BranchId);

        _logger.LogInformation("Login successful for user: {Username} at Branch: {BranchId}", request.Username, tokenDetails.Item2);

        // الحصول على اسم الفرع إذا كان موجوداً
        string? branchName = null;
        if (tokenDetails.Item2.HasValue)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(tokenDetails.Item2.Value);
            branchName = branch?.Name;
        }

        return new LoginResponseDto
        {
            Token = tokenDetails.Item1,
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            RoleName = role.Name,
            Email = user.Email,
            BranchId = tokenDetails.Item2,
            BranchName = branchName,
            // ERP: بيانات الموظف المرتبط بهذا الحساب
            EmployeeId = tokenDetails.Item3,
            EmployeeCode = tokenDetails.Item4,
            EmployeeName = tokenDetails.Item5,
            // ERP: الفروع المتاحة
            AllowedBranchIds = tokenDetails.Item6
        };
    }

    /// <summary>
    /// تبديل الفرع النشط وإرجاع توكن جديد
    /// </summary>
    public async Task<LoginResponseDto> SwitchBranchAsync(int userId, int newBranchId)
    {
        // 1. الحصول على المستخدم
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null || user.IsDeleted || user.Status != Core.Enums.UserStatus.Active)
        {
            throw new UnauthorizedAccessException("المستخدم غير موجود أو غير نشط");
        }

        // 2. الحصول على دور المستخدم
        var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId);
        if (role == null)
        {
            throw new InvalidOperationException("دور المستخدم غير موجود");
        }

        // 3. التحقق من صلاحية وصول المستخدم لهذا الفرع
        var assignments = await _unitOfWork.EmployeeBranchAssignments.GetActiveAssignmentsByUserIdAsync(userId);
        var allowedBranchIds = assignments.Select(a => a.BranchId).ToList();

        if (role.Name != "Admin" && !allowedBranchIds.Contains(newBranchId))
        {
            throw new UnauthorizedAccessException("ليس لديك صلاحية الدخول لهذا الفرع");
        }

        // 4. توليد التوكن الجديد مع الفرع المحدد
        var tokenDetails = await GenerateJwtTokenAsync(user, role.Name, newBranchId);

        _logger.LogInformation("Branch switched successfully for user: {Username} to Branch: {BranchId}", user.Username, newBranchId);

        string? branchName = null;
        if (tokenDetails.Item2.HasValue)
        {
            var branch = await _unitOfWork.Branches.GetByIdAsync(tokenDetails.Item2.Value);
            branchName = branch?.Name;
        }

        return new LoginResponseDto
        {
            Token = tokenDetails.Item1,
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            RoleName = role.Name,
            Email = user.Email,
            BranchId = tokenDetails.Item2,
            BranchName = branchName,
            EmployeeId = tokenDetails.Item3,
            EmployeeCode = tokenDetails.Item4,
            EmployeeName = tokenDetails.Item5,
            AllowedBranchIds = tokenDetails.Item6
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
    private async Task<(string, int?, int?, string?, string?, List<int>)> GenerateJwtTokenAsync(Core.Entities.User user, string roleName, int? requestedBranchId = null)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.GivenName, user.FullName),
            new Claim(ClaimTypes.Role, roleName),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim("UserId", user.Id.ToString()),
            new Claim("RoleId", user.RoleId.ToString())
        };

        // الحصول على الصلاحيات الفعلية للمستخدم
        var permissions = await _permissionService.GetUserEffectivePermissionsAsync(user.Id);
        if (permissions.Any())
        {
            claims.Add(new Claim("Permissions", string.Join(",", permissions)));
        }

        // الحصول على الفروع المتاحة للمستخدم
        var assignments = await _unitOfWork.EmployeeBranchAssignments.GetActiveAssignmentsByUserIdAsync(user.Id);
        var allowedBranchIds = assignments.Select(a => a.BranchId).ToList();

        if (allowedBranchIds.Any())
        {
            claims.Add(new Claim("AllowedBranches", string.Join(",", allowedBranchIds)));
        }

        // الحصول على تعيين الفرع النهائي
        int? finalBranchId = null;

        if (requestedBranchId.HasValue && (roleName == "Admin" || allowedBranchIds.Contains(requestedBranchId.Value)))
        {
            finalBranchId = requestedBranchId.Value;
        }
        else
        {
            // استخدم الفرع الافتراضي، وإلا فاستخدم أول فرع متاح
            finalBranchId = user.DefaultBranchId ?? allowedBranchIds.FirstOrDefault();
        }

        claims.Add(new Claim("BranchId", finalBranchId?.ToString() ?? ""));

        // ERP: ربط المستخدم بموظفه في هذا الفرع النشط
        int? employeeId = null;
        string? employeeCode = null;
        string? employeeName = null;
        
        // جلب جميع الموظفين المرتبطين بهذا المستخدم (لو كان هناك أكثر من واحد)
        var employees = await _unitOfWork.Employees.GetByUserIdAsync(user.Id);
        var employee = employees.FirstOrDefault(e => e.BranchId == finalBranchId) ?? employees.FirstOrDefault();
        
        if (employee != null)
        {
            employeeId = employee.Id;
            employeeCode = employee.EmployeeCode;
            employeeName = employee.FullName;
            claims.Add(new Claim("EmployeeId", employee.Id.ToString()));
            claims.Add(new Claim("EmployeeCode", employee.EmployeeCode ?? ""));
            claims.Add(new Claim("EmployeeName", employee.FullName ?? ""));
        }

        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "480");
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), finalBranchId, employeeId, employeeCode, employeeName, allowedBranchIds);
    }
}
