using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[Authorize]
public class IncidentController : Controller
{
    private readonly IIncidentService _incidentService;
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;

    public IncidentController(IIncidentService incidentService, IUserService userService, INotificationService notificationService)
    {
        _incidentService = incidentService;
        _userService = userService;
        _notificationService = notificationService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    // ═══════════════════════════════════════════════════════════════════════
    // GUARDS & CLIENTS — Submit Incident / Complaint
    // ═══════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Roles = "Guard,Client,VIP Client,VIP")]
    public IActionResult Submit()
    {
        var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "Guard";
        ViewBag.UserRole = role;
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Guard,Client,VIP Client,VIP")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(string title, string description)
    {
        var userId = GetUserId();
        var userName = User.Identity?.Name ?? "Unknown User";
        var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "Guard";
        
        string type = (userRole == "Guard") ? "Incident" : "Complaint";

        var result = await _incidentService.SubmitIncidentAsync(userId, userName, userRole, type, title, description);

        if (result.Success)
        {
            TempData["Success"] = $"✅ {type} submitted successfully. An administrator will review it shortly.";

            // 🔔 Real-time: Notify ALL Admins instantly
            await _notificationService.NotifyRoleAsync(
                roleName: "Admin",
                title: $"New {type} Submitted",
                message: $"'{title}' was reported by {userName}. Requires your review.",
                type: "Warning"
            );
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        if (!result.Success) return View();
        return RedirectToAction("MyReports");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GUARDS & CLIENTS — View Own Reports
    // ═══════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Roles = "Guard,Client,VIP Client,VIP")]
    public async Task<IActionResult> MyReports()
    {
        var reports = await _incidentService.GetMyReportsAsync(GetUserId());
        var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "Guard";
        ViewBag.UserRole = userRole;
        return View(reports.OrderByDescending(r => r.CreatedAt).ToList());
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ADMIN — Incident Panel
    // ═══════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminPanel(string filter = "All")
    {
        var incidents = filter switch
        {
            "Open" => await _incidentService.GetIncidentsByStatusAsync("Open"),
            "Resolved" => await _incidentService.GetIncidentsByStatusAsync("Resolved"),
            _ => await _incidentService.GetAllIncidentsAsync()
        };

        ViewBag.Filter = filter;
        ViewBag.OpenCount = incidents.Count(i => i.Status == "Open");
        ViewBag.ResolvedCount = incidents.Count(i => i.Status == "Resolved");

        return View(incidents.OrderByDescending(i => i.CreatedAt).ToList());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(string incidentId, string resolutionNotes)
    {
        var adminId = GetUserId();
        var adminName = User.Identity?.Name ?? "Administrator";
        var result = await _incidentService.ResolveIncidentAsync(incidentId, adminId, resolutionNotes);

        if (result.Success)
        {
            TempData["Success"] = "Incident has been resolved successfully.";

            // 🔔 Real-time: Notify the original reporter that their case is closed
            var incident = await _incidentService.GetIncidentByIdAsync(incidentId);
            if (incident != null && !string.IsNullOrEmpty(incident.ReportedByUserId))
            {
                await _notificationService.CreateNotificationAsync(
                    userId: incident.ReportedByUserId,
                    title: "Your Report Has Been Resolved",
                    message: $"Your {incident.Type} '{incident.Title}' has been reviewed and closed by {adminName}. Notes: {resolutionNotes}",
                    type: "Info"
                );
            }
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction("AdminPanel");
    }
}
