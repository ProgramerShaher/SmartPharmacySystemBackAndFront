namespace SmartPharmacySystem.ApiDTOs;

/// <summary>
/// كائن نقل البيانات للمستخدم في API.
/// يُستخدم هذا الكلاس لتسلسل الطلبات والاستجابات في API.
/// </summary>
public class UserApiDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}