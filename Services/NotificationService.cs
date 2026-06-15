using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Hubs;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IUnitOfWork unitOfWork, IHubContext<NotificationHub> hubContext)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
    }

    public async Task CreateNotificationAsync(string userId, string title, string message, string type = "Info")
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            CreatedAt = System.DateTime.UtcNow,
            IsRead = false,
            Expiration = System.DateTime.UtcNow.AddDays(30)
        };
        await _unitOfWork.Notifications.AddAsync(notification);
        // Push to client via SignalR group (userId)
        await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", notification);
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20)
    {
        // Simple pagination using repository (assumes method exists; otherwise fetch all and slice)
        var all = await _unitOfWork.Notifications.GetByUserIdAsync(userId);
        var skip = (page - 1) * pageSize;
        return all.Skip(skip).Take(pageSize).ToList();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(string notificationId)
    {
        await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
    }
}
