using Vanguard_Engine.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vanguard_Engine.Repositories;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<List<Notification>> GetByUserIdAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(string notificationId);
}
