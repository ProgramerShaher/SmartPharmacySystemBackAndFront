using Microsoft.AspNetCore.SignalR;

namespace SmartPharmacySystem.Infrastructure.Hubs;

/// <summary>
/// SignalR Hub for broadcasting pharmacy-wide notifications.
/// Moved to Infrastructure to resolve circular dependencies.
/// </summary>
public class NotificationHub : Hub
{
    public async Task SendNotification(string title, string message, string severity)
    {
        await Clients.All.SendAsync("ReceiveNotification", new { title, message, severity, timestamp = DateTime.UtcNow });
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}
