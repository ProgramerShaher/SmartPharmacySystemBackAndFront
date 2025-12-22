namespace SmartPharmacySystem.Application.DTOs.User;

/// <summary>
/// كائن نقل البيانات للمستخدم.
/// يُستخدم هذا الكلاس لنقل بيانات المستخدم بين الطبقات.
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }
}