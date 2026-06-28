using Vanguard_Engine.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vanguard_Engine.Repositories;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<List<Notification>> GetByUserIdAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(string notificationId);

    /// <summary>MODULE 9: Marks all unread notifications for a user as read.</summary>
    Task MarkAllReadAsync(string userId);

    /// <summary>MODULE 9: Deletes all expired notifications for a user.</summary>
    Task DeleteExpiredAsync(string userId);
}

