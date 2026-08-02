namespace SmartPharmacySystem.Application.DTOs.Auth;

/// <summary>
/// طلب تسجيل الدخول
/// Login request DTO
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// اسم المستخدم
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// كلمة المرور
    /// Password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// معرف الفرع (اختياري) لتسجيل الدخول كفرع معين
    /// Optional Branch ID to login as a specific branch
    /// </summary>
    public int? BranchId { get; set; }
}
