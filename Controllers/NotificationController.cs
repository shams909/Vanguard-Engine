using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // GET /api/notifications?userId=xxx
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] string? userId)
    {
        var uid = userId ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var notifications = await _notificationService.GetUserNotificationsAsync(uid, 1, 30);
        var unread = notifications.Count(n => !n.IsRead);
        return Ok(new { notifications, unreadCount = unread });
    }

    // GET /api/notifications/unread-count?userId=xxx
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount([FromQuery] string? userId)
    {
        var uid = userId ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
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

    // POST /api/notifications  (for testing / internal use)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotificationRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId) || string.IsNullOrWhiteSpace(req.Message))
            return BadRequest("UserId and Message are required.");

        await _notificationService.CreateNotificationAsync(
            req.UserId,
            req.Title ?? "Notification",
            req.Message,
            req.Type ?? "Info");

        return Created("", null);
    }
}

public record CreateNotificationRequest(
    string? UserId,
    string? Title,
    string? Message,
    string? Type);
