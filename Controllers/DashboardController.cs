using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IUserService _userService;
    private readonly IGuardApplicationService _guardService;
    private readonly IHiringService _hiringService;
    private readonly IClientRequestService _requestService;
    private readonly IVIPRequestService _vipRequestService;
    private readonly IGuardShiftService _shiftService;

    public DashboardController(
        IUserService userService,
        IGuardApplicationService guardService,
        IHiringService hiringService,
        IClientRequestService requestService,
        IVIPRequestService vipRequestService,
        IGuardShiftService shiftService)
    {
        _userService = userService;
        _guardService = guardService;
        _hiringService = hiringService;
        _requestService = requestService;
        _vipRequestService = vipRequestService;
        _shiftService = shiftService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "Guard";

        return role switch
        {
            "Admin" => RedirectToAction(nameof(Admin)),
            "Recruiter" => RedirectToAction(nameof(Recruiter)),
            "Guard" => RedirectToAction(nameof(Guard)),
            "Client" => RedirectToAction(nameof(Client)),
            "VIP Client" or "VIP" => RedirectToAction(nameof(Vip)),
            _ => RedirectToAction("AccessDenied", "Auth")
        };
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Admin()
    {
        var allUsers = await _userService.GetAllAsync(1, 100);
        var allNotices = await _hiringService.GetAllAsync();
        var allApps = await _guardService.GetAllApplicationsAsync();

        ViewBag.TotalUsers = allUsers.Count;
        ViewBag.TotalNotices = allNotices.Count;
        ViewBag.TotalApplications = allApps.Count;
        ViewBag.PendingApplications = allApps.Count(a => a.Status == "Pending");
        ViewBag.ApprovedApplications = allApps.Count(a => a.Status == "Approved");

        return View();
    }

    [HttpGet]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> Recruiter()
    {
        var allApps = await _guardService.GetAllApplicationsAsync();

        ViewBag.PendingApps = allApps.Count(a => a.Status == "Pending");
        ViewBag.ApprovedApps = allApps.Count(a => a.Status == "Approved");
        ViewBag.RejectedApps = allApps.Count(a => a.Status == "Rejected");
        ViewBag.TotalApps = allApps.Count;

        var recentPending = allApps.Where(a => a.Status == "Pending").OrderByDescending(a => a.CreatedAt).Take(5).ToList();
        return View(recentPending);
    }

    [HttpGet]
    [Authorize(Roles = "Guard")]
    public async Task<IActionResult> Guard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var myApps = await _guardService.GetMyApplicationsAsync(userId);

        // Guard profile status
        var generalProfile = myApps.FirstOrDefault(a => string.IsNullOrEmpty(a.JobId) || a.JobId == "");
        ViewBag.GuardStatus = generalProfile?.GuardStatus ?? "Available";

        // Active ClientRequest assignment
        var allRequests = await _requestService.GetAllRequestsAsync();
        var activeAssignment = allRequests.FirstOrDefault(r =>
            r.Status == "Approved" && r.AssignedGuardIds != null && r.AssignedGuardIds.Contains(userId));
        ViewBag.ActiveAssignment = activeAssignment;

        // Active VIP Mission assignment
        var allVipRequests = await _vipRequestService.GetAllRequestsAsync();
        var vipMission = allVipRequests.FirstOrDefault(r =>
            (r.Status == "Assigned" || r.Status == "Active") &&
            r.AssignedGuardIds != null && r.AssignedGuardIds.Contains(userId));
        ViewBag.VipMission = vipMission;

        // Current shift (for real check-in/out widget)
        var activeShift = await _shiftService.GetActiveShiftAsync(userId);
        ViewBag.ActiveShift = activeShift;

        return View(myApps);
    }

    [HttpGet]
    [Authorize(Roles = "Client")]
    public IActionResult Client()
    {
        return View();
    }

    [HttpGet]
    [Authorize(Roles = "VIP Client,VIP")]
    public async Task<IActionResult> Vip()
    {
        var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var allRequests = await _vipRequestService.GetRequestsByClientAsync(clientId);
        var counts = await _vipRequestService.GetStatusCountsAsync(clientId);

        ViewBag.StatusCounts = counts;
        ViewBag.ActiveRequests  = allRequests.Where(r => r.Status is "Approved" or "Assigned" or "Active").ToList();
        ViewBag.PendingRequests = allRequests.Where(r => r.Status == "Pending").ToList();
        ViewBag.RecentHistory   = allRequests.Where(r => r.Status is "Completed" or "Rejected" or "Cancelled")
                                             .OrderByDescending(r => r.CreatedAt).Take(5).ToList();
        return View();
    }
}
