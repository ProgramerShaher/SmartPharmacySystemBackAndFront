using SmartPharmacySystem.Core.Enums;

namespace SmartPharmacySystem.Application.DTOs.Notifications;

/// <summary>
/// كائن نقل البيانات لعرض إشعار.
/// </summary>
public class NotificationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public NotificationType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string TypeIcon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
}
