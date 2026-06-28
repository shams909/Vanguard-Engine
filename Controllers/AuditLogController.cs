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

    // GET /AuditLog/Export
    [HttpGet("Export")]
    public async Task<IActionResult> Export()
    {
        var logs = await _auditLogService.GetRecentActivityAsync(1000); // Export top 1000 for now
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("Time,Entity Type,Entity ID,Action,From,To,Performed By,Notes");

        foreach (var log in logs)
        {
            builder.AppendLine($"\"{log.CreatedAt:yyyy-MM-dd HH:mm:ss}\",\"{log.EntityType}\",\"{log.EntityId}\",\"{log.Action}\",\"{log.FromValue}\",\"{log.ToValue}\",\"{log.PerformedByRole}\",\"{log.Notes?.Replace("\"", "\"\"")}\"");
        }

        return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"AuditLogs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }
}
