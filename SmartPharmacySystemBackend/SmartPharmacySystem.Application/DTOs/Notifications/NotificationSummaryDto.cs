namespace SmartPharmacySystem.Application.DTOs.Notifications;

/// <summary>
/// كائن نقل البيانات لعرض ملخص الإشعارات (للجرس).
/// </summary>
public class NotificationSummaryDto
{
    public int UnreadCount { get; set; }
    public List<NotificationDto> RecentNotifications { get; set; } = new();
}
