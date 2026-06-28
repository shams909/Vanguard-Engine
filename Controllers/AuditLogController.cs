using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

/// <summary>
/// MODULE 11: Admin-only audit trail viewer.
/// </summary>
[Authorize(Roles = "Admin")]
public class AuditLogController : BaseController
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    // GET /AuditLog — global recent feed
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] int limit = 50)
    {
        var logs = await _auditLogService.GetRecentActivityAsync(limit);
        return View(logs);
    }

    // GET /AuditLog/Entity?type=ClientRequest&id=xxx
    [HttpGet]
    public async Task<IActionResult> Entity([FromQuery] string type, [FromQuery] string id)
    {
        if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(id))
            return BadRequest("Entity type and ID are required.");

        var logs = await _auditLogService.GetEntityHistoryAsync(type, id);
        ViewBag.EntityType = type;
        ViewBag.EntityId   = id;
        return View(logs);
    }

    // GET /AuditLog/UserActivity?id=xxx
    [HttpGet]
    public async Task<IActionResult> UserActivity([FromQuery] string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("User ID is required.");

        var logs = await _auditLogService.GetUserActivityAsync(id);
        ViewBag.UserId = id;
        return View(logs);
    }
}
