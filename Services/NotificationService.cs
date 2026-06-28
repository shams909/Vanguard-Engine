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

    // ── Send to a specific user ───────────────────────────────────────────────

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

        try { await _unitOfWork.Notifications.AddAsync(notification); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "[Notifications] Failed to persist notification for user {UserId}. " +
                "Ensure 'notifications' collection exists in Appwrite.", userId);
        }

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
            _logger.LogWarning(ex,
                "[Notifications] SignalR push failed for user {UserId}.", userId);
        }
    }

    // ── Send to every user with a given role name (e.g. "Admin") ─────────────

    public async Task NotifyRoleAsync(string roleName, string title, string message, string type = "Info")
    {
        try
        {
            // 1. Find the role document by name
            var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
            if (role == null)
            {
                _logger.LogWarning("[Notifications] Role '{Role}' not found – skipping admin notification.", roleName);
                return;
            }

            // 2. Page through users that have this roleId
            int page = 1;
            const int pageSize = 50;
            var adminIds = new List<string>();

            while (true)
            {
                var users = await _unitOfWork.Users.GetPagedAsync(page, pageSize);
                if (users == null || users.Count == 0) break;

                foreach (var u in users)
                    if (u.RoleId == role.Id && !string.IsNullOrWhiteSpace(u.Id))
                        adminIds.Add(u.Id);

                if (users.Count < pageSize) break;
                page++;
            }

            if (adminIds.Count == 0)
            {
                _logger.LogWarning("[Notifications] No users found with role '{Role}'.", roleName);
                return;
            }

            // 3. Send notification to every admin
            var tasks = adminIds.Select(id => CreateNotificationAsync(id, title, message, type));
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Notifications] NotifyRoleAsync failed for role '{Role}'.", roleName);
        }
    }

    // ── Queries ───────────────────────────────────────────────────────────────

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
        try   { return await _unitOfWork.Notifications.GetUnreadCountAsync(userId); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Notifications] Failed to get unread count for user {UserId}.", userId);
            return 0;
        }
    }

    public async Task MarkAsReadAsync(string notificationId)
    {
        try   { await _unitOfWork.Notifications.MarkAsReadAsync(notificationId); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Notifications] Failed to mark {Id} as read.", notificationId);
        }
    }

    public async Task MarkAllReadAsync(string userId)
    {
        try   { await _unitOfWork.Notifications.MarkAllReadAsync(userId); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Notifications] MarkAllReadAsync failed for user {UserId}.", userId);
        }
    }

    public async Task DeleteExpiredAsync(string userId)
    {
        try   { await _unitOfWork.Notifications.DeleteExpiredAsync(userId); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Notifications] DeleteExpiredAsync failed for user {UserId}.", userId);
        }
    }

    public async Task DeleteAsync(string notificationId)
    {
        try { await _unitOfWork.Notifications.DeleteAsync(notificationId); }
        catch (Exception ex) { _logger.LogWarning(ex, "[Notifications] DeleteAsync failed."); }
    }

    public async Task DeleteManyAsync(List<string> notificationIds)
    {
        try { await _unitOfWork.Notifications.DeleteManyAsync(notificationIds); }
        catch (Exception ex) { _logger.LogWarning(ex, "[Notifications] DeleteManyAsync failed."); }
    }
}
