using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Hubs;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task CreateNotificationAsync(string userId, string title, string message, string type = "Info")
    {
        if (string.IsNullOrWhiteSpace(userId)) return;

        var notification = new Notification
        {
            UserId     = userId,
            Title      = title,
            Message    = message,
            Type       = type,
            CreatedAt  = DateTime.UtcNow,
            IsRead     = false,
            Expiration = DateTime.UtcNow.AddDays(30)
        };

        // Persist — silent fail so notifications never crash the main flow
        try
        {
            await _unitOfWork.Notifications.AddAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Notifications] Failed to persist notification for user {UserId}. " +
                                   "Check that the 'notifications' collection exists in Appwrite.", userId);
        }

        // Real-time push — also silent fail
        try
        {
            await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", new
            {
                id        = notification.Id,
                title     = notification.Title,
                message   = notification.Message,
                type      = notification.Type,
                createdAt = notification.CreatedAt,
                isRead    = notification.IsRead
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Notifications] Failed to push SignalR notification for user {UserId}.", userId);
        }
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var all  = await _unitOfWork.Notifications.GetByUserIdAsync(userId);
            var skip = (page - 1) * pageSize;
            return all.Skip(skip).Take(pageSize).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Notifications] Failed to fetch notifications for user {UserId}.", userId);
            return new List<Notification>();
        }
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        try
        {
            return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Notifications] Failed to get unread count for user {UserId}.", userId);
            return 0;
        }
    }

    public async Task MarkAsReadAsync(string notificationId)
    {
        try
        {
            await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Notifications] Failed to mark notification {Id} as read.", notificationId);
        }
    }
}
