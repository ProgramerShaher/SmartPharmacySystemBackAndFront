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

    /// <summary>
    /// معرف الفرع الحالي
    /// Current Branch ID
    /// </summary>
    public int? BranchId { get; set; }

    /// <summary>
    /// اسم الفرع الحالي
    /// Current Branch Name
    /// </summary>
    public string? BranchName { get; set; }

    /// <summary>
    /// معرف الموظف المرتبط (إن وجد) - ERP Style
    /// Employee ID linked to this user (if any)
    /// </summary>
    public int? EmployeeId { get; set; }

    /// <summary>
    /// كود الموظف
    /// Employee code
    /// </summary>
    public string? EmployeeCode { get; set; }

    /// <summary>
    /// اسم الموظف الكامل (من سجل الموظف)
    /// Employee full name (from employee record)
    /// </summary>
    public string? EmployeeName { get; set; }

    /// <summary>
    /// الفروع المتاحة للمستخدم
    /// </summary>
    public List<int> AllowedBranchIds { get; set; } = new();
}
