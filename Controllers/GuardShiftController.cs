using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[Authorize]
public class GuardShiftController : Controller
{
    private readonly IGuardShiftService _shiftService;
    private readonly IGuardApplicationService _guardService;

    public GuardShiftController(IGuardShiftService shiftService, IGuardApplicationService guardService)
    {
        _shiftService = shiftService;
        _guardService = guardService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    // ═══════════════════════════════════════════════════════════════════════
    // GUARD — Check In / Check Out
    // ═══════════════════════════════════════════════════════════════════════

    [HttpPost]
    [Authorize(Roles = "Guard")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn()
    {
        var guardId = GetUserId();

        // Retrieve guard's full name — try general profile first, then any approved app
        var allApps  = await _guardService.GetMyApplicationsAsync(guardId);
        var profile  = allApps.FirstOrDefault(a => string.IsNullOrEmpty(a.JobId) || a.JobId == "")
                    ?? allApps.FirstOrDefault(a => a.Status == "Approved");
        var guardName = profile?.FullName ?? User.Identity?.Name ?? "Unknown Officer";

        var result = await _shiftService.CheckInAsync(guardId, guardName);

        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "✅ Shift started. You are now checked in." : result.Error;

        return RedirectToAction("Guard", "Dashboard");
    }

    [HttpPost]
    [Authorize(Roles = "Guard")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(string shiftId)
    {
        var result = await _shiftService.CheckOutAsync(shiftId, GetUserId());

        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "Shift completed. Duration saved to your history." : result.Error;

        return RedirectToAction("Guard", "Dashboard");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GUARD — Shift History
    // ═══════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Roles = "Guard")]
    public async Task<IActionResult> History()
    {
        var shifts = await _shiftService.GetShiftHistoryAsync(GetUserId());
        return View(shifts);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ADMIN — Attendance Panel
    // ═══════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<IActionResult> AdminPanel(string filter = "All")
    {
        var shifts = filter switch
        {
            "Active"    => await _shiftService.GetActiveShiftsAsync(),
            "Completed" => await _shiftService.GetCompletedShiftsAsync(),
            _           => await _shiftService.GetAllShiftsAsync()
        };

        var stats = await _shiftService.GetTodayStatsAsync();
        ViewBag.Filter          = filter;
        ViewBag.ActiveCount     = stats.Active;
        ViewBag.CompletedToday  = stats.CompletedToday;
        ViewBag.AvgDuration     = stats.AverageDurationMinutes;

        return View(shifts);
    }
}
