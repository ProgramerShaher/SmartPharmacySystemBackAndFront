namespace SmartPharmacySystem.Application.DTOs.User;

public class UserPermissionsResponseDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    
    /// <summary>
    /// قائمة بالأكواد النهائية المسموح بها للمستخدم (بعد دمج الدور مع الاستثناءات)
    /// </summary>
    public List<string> EffectivePermissions { get; set; } = new();
}
