using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
[IgnoreAntiforgeryToken]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private string GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 30)
    {
        var uid = GetCurrentUserId();
        if (string.IsNullOrEmpty(uid)) return Unauthorized();

        var notifications = await _notificationService.GetUserNotificationsAsync(uid, page, pageSize);
        var unreadCount   = await _notificationService.GetUnreadCountAsync(uid);
        return Ok(new { notifications, unreadCount });
    }

    // GET /api/notifications/unread-count
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var uid = GetCurrentUserId();
        if (string.IsNullOrEmpty(uid)) return Unauthorized();

        var count = await _notificationService.GetUnreadCountAsync(uid);
        return Ok(new { count });
    }

    // PATCH /api/notifications/{id}/read
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkRead(string id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return NoContent();
    }

    // MODULE 9: PATCH /api/notifications/read-all
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var uid = GetCurrentUserId();
        if (string.IsNullOrEmpty(uid)) return Unauthorized();

        await _notificationService.MarkAllReadAsync(uid);
        return NoContent();
    }

    // MODULE 9: DELETE /api/notifications/expired
    [HttpDelete("expired")]
    public async Task<IActionResult> DeleteExpired()
    {
        var uid = GetCurrentUserId();
        if (string.IsNullOrEmpty(uid)) return Unauthorized();

        await _notificationService.DeleteExpiredAsync(uid);
        return NoContent();
    }

    // DELETE /api/notifications/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _notificationService.DeleteAsync(id);
        return NoContent();
    }

    // POST /api/notifications/bulk-delete
    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDelete([FromBody] List<string> ids)
    {
        if (ids != null && ids.Any())
        {
            await _notificationService.DeleteManyAsync(ids);
        }
        return NoContent();
    }
}
