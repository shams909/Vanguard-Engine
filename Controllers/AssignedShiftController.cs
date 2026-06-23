using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[Authorize]
public class AssignedShiftController : Controller
{
    private readonly IAssignedShiftService _assignedShiftService;
    private readonly IGuardApplicationService _guardAppService;

    public AssignedShiftController(IAssignedShiftService assignedShiftService, IGuardApplicationService guardAppService)
    {
        _assignedShiftService = assignedShiftService;
        _guardAppService = guardAppService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    // ═══════════════════════════════════════════════════════════════════════
    // ADMIN — Shift Management Panel
    // ═══════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<IActionResult> AdminPanel()
    {
        var shifts = await _assignedShiftService.GetAllAssignedShiftsAsync();
        var allApps = await _guardAppService.GetAllApplicationsAsync();
        
        // Filter guards that have been approved or accepted
        ViewBag.ApprovedGuards = allApps.Where(a => a.Status == "Approved" || a.Status == "Accepted").ToList();
        return View(shifts.OrderByDescending(s => s.ShiftDate).ThenByDescending(s => s.StartTime).ToList());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Recruiter")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignShift(string guardId, string guardName, string shiftDate, string startTime, string endTime)
    {
        var result = await _assignedShiftService.AssignShiftAsync(guardId, guardName, shiftDate, startTime, endTime);

        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "✅ Shift successfully assigned." : result.Error;

        return RedirectToAction("AdminPanel");
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Recruiter")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string shiftId, string status)
    {
        var result = await _assignedShiftService.UpdateShiftStatusAsync(shiftId, status);
        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? $"Shift status updated to {status}." : result.Error;

        return RedirectToAction("AdminPanel");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GUARD — View Assigned Schedule
    // ═══════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Roles = "Guard")]
    public async Task<IActionResult> GuardSchedule()
    {
        var shifts = await _assignedShiftService.GetGuardScheduleAsync(GetUserId());
        return View(shifts.OrderByDescending(s => s.ShiftDate).ThenByDescending(s => s.StartTime).ToList());
    }
}
