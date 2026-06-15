using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vanguard_Engine.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("unread")]
    public async Task<ActionResult<List<Notification>>> GetUnread([FromQuery] string userId)
    {
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, 1, 20);
        return Ok(notifications);
    }

    [HttpPost("markread")]
    public async Task<IActionResult> MarkRead([FromBody] MarkReadRequest request)
    {
        await _notificationService.MarkAsReadAsync(request.NotificationId);
        return NoContent();
    }
}

public class MarkReadRequest
{
    public string NotificationId { get; set; } = string.Empty;
}
