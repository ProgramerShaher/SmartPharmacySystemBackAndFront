namespace SmartPharmacySystem.Application.DTOs.Auth;

/// <summary>
/// طلب تغيير كلمة المرور
/// Change password request DTO
/// </summary>
public class ChangePasswordDto
{
    /// <summary>
    /// كلمة المرور القديمة
    /// Old password
    /// </summary>
    public string OldPassword { get; set; } = string.Empty;

    /// <summary>
    /// كلمة المرور الجديدة
    /// New password
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// تأكيد كلمة المرور الجديدة
    /// Confirm new password
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}
