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

    /// <summary>
    /// معرف الدور
    /// Role ID
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// اسم الدور
    /// Role name
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// وصف الدور
    /// Role description
    /// </summary>
    public string? RoleDescription { get; set; }

    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }
}