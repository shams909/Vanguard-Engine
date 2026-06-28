using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[Authorize]
public class GuardShiftController : BaseController
{
    private readonly IGuardShiftService _shiftService;
    private readonly IGuardApplicationService _guardService;
    private readonly INotificationService _notificationService;

    public GuardShiftController(IGuardShiftService shiftService, IGuardApplicationService guardService, INotificationService notificationService)
    {
        _shiftService = shiftService;
        _guardService = guardService;
        _notificationService = notificationService;
    }


    // ═══════════════════════════════════════════════════════════════════════
    // GUARD — Check In / Check Out
    // ═══════════════════════════════════════════════════════════════════════

    [HttpPost]
    [Authorize(Roles = "Guard")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(string assignedShiftId)
    {
        var guardId = GetUserId();

        // Retrieve guard's full name — try general profile first, then any approved app
        var allApps  = await _guardService.GetMyApplicationsAsync(guardId);
        var profile  = allApps.FirstOrDefault(a => string.IsNullOrEmpty(a.JobId) || a.JobId == "")
                    ?? allApps.FirstOrDefault(a => a.Status == "Approved");
        var guardName = profile?.FullName ?? User.Identity?.Name ?? "Unknown Officer";

        var result = await _shiftService.CheckInAsync(guardId, guardName, assignedShiftId);

        if (result.Success)
        {
            TempData["Success"] = "✅ Shift started. You are now checked in.";
            // 🔔 Real-time: Notify Admins of live check-in
            await _notificationService.NotifyRoleAsync(
                roleName: "Admin",
                title: "Guard Checked In",
                message: $"{guardName} has checked in and started their shift.",
                type: "Info"
            );
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction("Guard", "Dashboard");
    }

    [HttpPost]
    [Authorize(Roles = "Guard")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(string shiftId)
    {
        var guardName = User.Identity?.Name ?? "Unknown Officer";
        var result = await _shiftService.CheckOutAsync(shiftId, GetUserId());

        if (result.Success)
        {
            TempData["Success"] = "Shift completed. Duration saved to your history.";
            // 🔔 Real-time: Notify Admins of check-out
            await _notificationService.NotifyRoleAsync(
                roleName: "Admin",
                title: "Guard Checked Out",
                message: $"{guardName} has completed their shift and checked out.",
                type: "Info"
            );
        }
        else
        {
            TempData["Error"] = result.Error;
        }

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
