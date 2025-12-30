using Microsoft.AspNetCore.SignalR;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Hubs;

namespace SmartPharmacySystem.Infrastructure.Services;

/// <summary>
/// Implementation of INotificationService using SignalR.
/// تنفيذ خدمة التنبيهات باستخدام SignalR.
/// </summary>
public class NotificationService(IHubContext<NotificationHub> hubContext) : INotificationService
{
    public async Task SendNotificationAsync(string title, string message, string severity)
    {
        await hubContext.Clients.All.SendAsync("ReceiveNotification", new 
        { 
            title, 
            message, 
            severity, 
            timestamp = DateTime.UtcNow 
        });
    }
}
