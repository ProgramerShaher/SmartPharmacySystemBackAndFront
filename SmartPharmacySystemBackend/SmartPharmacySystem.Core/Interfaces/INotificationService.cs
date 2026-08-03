namespace SmartPharmacySystem.Core.Interfaces;

/// <summary>
/// Interface for sending real-time notifications.
/// واجهة لإرسال التنبيهات في الوقت الفعلي.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification to all connected clients.
    /// يرسل تنبيهاً لجميع المستخدمين المتصلين.
    /// </summary>
    Task SendNotificationAsync(string title, string message, string severity);
    Task NotifyBranchAsync(int branchId, SmartPharmacySystem.Core.Enums.NotificationType type, string title, string body, int? referenceId = null, string? referenceType = null);
    Task NotifyUserAsync(int userId, int? branchId, SmartPharmacySystem.Core.Enums.NotificationType type, string title, string body, int? referenceId = null, string? referenceType = null);
    
    Task<IEnumerable<SmartPharmacySystem.Core.Entities.Notification>> GetUnreadNotificationsAsync(int userId, int? branchId);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId, int? branchId);
}
