using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure.Data;
using SmartPharmacySystem.Infrastructure.Hubs;

namespace SmartPharmacySystem.Infrastructure.Services;

/// <summary>
/// Implementation of INotificationService using SignalR for real-time push
/// and ApplicationDbContext for persistence so notifications survive page reload.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ApplicationDbContext _context;

    public NotificationService(IHubContext<NotificationHub> hubContext, ApplicationDbContext context)
    {
        _hubContext = hubContext;
        _context = context;
    }

    /// <inheritdoc/>
    public async Task SendNotificationAsync(string title, string message, string severity)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
        {
            title,
            message,
            severity,
            timestamp = DateTime.UtcNow
        });
    }

    /// <inheritdoc/>
    public async Task NotifyBranchAsync(
        int branchId,
        NotificationType type,
        string title,
        string body,
        int? referenceId = null,
        string? referenceType = null)
    {
        var userIds = await _context.EmployeeBranchAssignments
            .Where(a => a.BranchId == branchId && a.IsActive)
            .Select(a => a.UserId)
            .ToListAsync();

        // Also notify all active Admins so they can see branch notifications
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole != null)
        {
            var adminUserIds = await _context.Users
                .Where(u => u.RoleId == adminRole.Id && u.Status == UserStatus.Active && !u.IsDeleted)
                .Select(u => u.Id)
                .ToListAsync();
            
            userIds.AddRange(adminUserIds);
        }

        userIds = userIds.Distinct().ToList();

        if (!userIds.Any()) return;

        var notifications = userIds.Select(uid => new Notification
        {
            UserId      = uid,
            BranchId    = branchId,
            Type        = type,
            Title       = title,
            Body        = body,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            IsRead      = false,
            CreatedAt   = DateTime.UtcNow
        }).ToList();

        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();

        // Push real-time to the branch group
        await _hubContext.Clients.Group($"branch_{branchId}").SendAsync("ReceiveNotification", new
        {
            title,
            message     = body,
            severity    = "info",
            type        = type.ToString(),
            referenceId,
            referenceType,
            timestamp   = DateTime.UtcNow
        });
    }

    /// <inheritdoc/>
    public async Task NotifyUserAsync(
        int userId,
        int? branchId,
        NotificationType type,
        string title,
        string body,
        int? referenceId = null,
        string? referenceType = null)
    {
        var notification = new Notification
        {
            UserId        = userId,
            BranchId      = branchId,
            Type          = type,
            Title         = title,
            Body          = body,
            ReferenceId   = referenceId,
            ReferenceType = referenceType,
            IsRead        = false,
            CreatedAt     = DateTime.UtcNow
        };

        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();

        // Push real-time to this specific user connection group
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", new
        {
            title,
            message       = body,
            severity      = "info",
            type          = type.ToString(),
            referenceId,
            referenceType,
            timestamp     = DateTime.UtcNow
        });
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(int userId, int? branchId)
    {
        return await _context.Notifications
            .Where(n => (n.UserId == userId || (branchId.HasValue && n.BranchId == branchId.Value)) && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId);
        if (notification != null && (notification.UserId == userId || notification.BranchId.HasValue))
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc/>
    public async Task MarkAllAsReadAsync(int userId, int? branchId)
    {
        var notifications = await _context.Notifications
            .Where(n => (n.UserId == userId || (branchId.HasValue && n.BranchId == branchId.Value)) && !n.IsRead)
            .ToListAsync();

        if (notifications.Any())
        {
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }
            await _context.SaveChangesAsync();
        }
    }
}
