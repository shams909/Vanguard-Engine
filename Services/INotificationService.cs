using System.Collections.Generic;
using System.Threading.Tasks;
using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface INotificationService
{
    /// <summary>Send a notification to a specific user.</summary>
    Task CreateNotificationAsync(string userId, string title, string message, string type = "Info");

    /// <summary>Send the same notification to every user who has the given role name (e.g. "Admin").</summary>
    Task NotifyRoleAsync(string roleName, string title, string message, string type = "Info");

    // ── Queries ───────────────────────────────────────────────────────────────
    Task<List<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);
    Task<int> GetUnreadCountAsync(string userId);

    // ── Commands ──────────────────────────────────────────────────────────────
    Task MarkAsReadAsync(string notificationId);

    /// <summary>MODULE 9: Marks every unread notification for a user as read in one operation.</summary>
    Task MarkAllReadAsync(string userId);

    /// <summary>MODULE 9: Deletes all notifications past their expiry date for housekeeping.</summary>
    Task DeleteExpiredAsync(string userId);

    Task DeleteAsync(string notificationId);
    Task DeleteManyAsync(List<string> notificationIds);
}
