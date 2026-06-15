using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Vanguard_Engine.Hubs;

public class NotificationHub : Hub
{
    // Clients join a group named after their user ID to receive personal notifications
    public async Task Subscribe(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }

    public async Task Unsubscribe(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
    }
}
