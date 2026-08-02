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
        // Fetch all active users in this branch
        var userIds = await _context.EmployeeBranchAssignments
            .Where(a => a.BranchId == branchId && a.IsActive)
            .Select(a => a.UserId)
            .Distinct()
            .ToListAsync();

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
}
