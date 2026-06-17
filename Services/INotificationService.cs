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

    Task<List<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(string notificationId);
}
