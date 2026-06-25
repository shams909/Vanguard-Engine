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

    public IncidentController(IIncidentService incidentService, IUserService userService)
    {
        _incidentService = incidentService;
        _userService = userService;
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

        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? $"✅ {type} submitted successfully. An administrator will review it shortly." : result.Error;

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
        var result = await _incidentService.ResolveIncidentAsync(incidentId, adminId, resolutionNotes);

        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "Incident has been resolved successfully." : result.Error;

        return RedirectToAction("AdminPanel");
    }
}
