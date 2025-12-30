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
}
