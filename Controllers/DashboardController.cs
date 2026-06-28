using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[Authorize]
public class DashboardController : BaseController
{
    private readonly IUserService             _userService;
    private readonly IGuardApplicationService _guardService;
    private readonly IHiringService           _hiringService;
    private readonly IClientRequestService    _requestService;
    private readonly IVIPRequestService       _vipRequestService;
    private readonly IGuardShiftService       _shiftService;
    private readonly IAssignedShiftService    _assignedShiftService;
    private readonly IRatingService           _ratingService;

    public DashboardController(
        IUserService             userService,
        IGuardApplicationService guardService,
        IHiringService           hiringService,
        IClientRequestService    requestService,
        IVIPRequestService       vipRequestService,
        IGuardShiftService       shiftService,
        IAssignedShiftService    assignedShiftService,
        IRatingService           ratingService)
    {
        _userService          = userService;
        _guardService         = guardService;
        _hiringService        = hiringService;
        _requestService       = requestService;
        _vipRequestService    = vipRequestService;
        _shiftService         = shiftService;
        _assignedShiftService = assignedShiftService;
        _ratingService        = ratingService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "Guard";
        return role switch
        {
            "Admin"      => RedirectToAction(nameof(Admin)),
            "Recruiter"  => RedirectToAction(nameof(Recruiter)),
            "Guard"      => RedirectToAction(nameof(Guard)),
            "Client"     => RedirectToAction(nameof(Client)),
            "VIP Client" => RedirectToAction(nameof(Vip)),
            _            => RedirectToAction("AccessDenied", "Auth")
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ADMIN DASHBOARD — Full 11 KPIs
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Admin()
    {
        var allUsers   = await _userService.GetAllAsync(1, 500);
        var allNotices = await _hiringService.GetAllAsync();
        var allApps    = await _guardService.GetAllApplicationsAsync();
        var allRequests    = await _requestService.GetAllRequestsAsync();
        var allVipRequests = await _vipRequestService.GetAllRequestsAsync();
        var allShifts      = await _assignedShiftService.GetAllAssignedShiftsAsync();

        // User KPIs
        ViewBag.TotalUsers     = allUsers.Count;
        ViewBag.TotalGuards    = allUsers.Count(u => u.GuardStatus != null);
        ViewBag.ActiveGuards   = allUsers.Count(u => u.GuardStatus == "Available" || u.GuardStatus == "Assigned" || u.GuardStatus == "OnDuty");
        ViewBag.SuspendedGuards = allUsers.Count(u => u.GuardStatus == "Suspended");

        // Application KPIs
        ViewBag.TotalApplications   = allApps.Count;
        ViewBag.PendingApplications = allApps.Count(a => a.Status == "Pending");
        ViewBag.ApprovedApplications = allApps.Count(a => a.Status == "Approved");

        // Request KPIs
        ViewBag.TotalClientRequests  = allRequests.Count;
        ViewBag.PendingRequests      = allRequests.Count(r => r.Status == "Pending");
        ViewBag.ActiveDeployments    = allRequests.Count(r => r.Status is "Assigned" or "Scheduled" or "Active" or "Partially Assigned");

        // VIP KPIs
        ViewBag.TotalVipRequests = allVipRequests.Count;
        ViewBag.ActiveVipMissions = allVipRequests.Count(r => r.Status is "Assigned" or "Scheduled" or "Active");

        // Hiring KPIs
        ViewBag.TotalNotices = allNotices.Count;
        ViewBag.OpenNotices  = allNotices.Count(n => n.Status == "Open");

        // Shift KPIs
        ViewBag.TodayShifts = allShifts.Count(s => s.ShiftDate == DateTime.UtcNow.ToString("yyyy-MM-dd"));

        // Recent pending applications for quick action
        ViewBag.RecentPendingApps = allApps
            .Where(a => a.Status == "Pending" && string.IsNullOrEmpty(a.JobId))
            .OrderByDescending(a => a.CreatedAt)
            .Take(5).ToList();

        return View();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RECRUITER DASHBOARD
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = "Recruiter")]
    public async Task<IActionResult> Recruiter()
    {
        var allApps    = await _guardService.GetAllApplicationsAsync();
        var allNotices = await _hiringService.GetAllAsync();

        ViewBag.PendingApps  = allApps.Count(a => a.Status == "Pending");
        ViewBag.ApprovedApps = allApps.Count(a => a.Status == "Approved");
        ViewBag.RejectedApps = allApps.Count(a => a.Status == "Rejected");
        ViewBag.TotalApps    = allApps.Count;

        // Shortlisted applications across all deployment posts
        ViewBag.ShortlistedApps = allApps.Count(a => a.Status == "Shortlisted");

        // Open notices this recruiter should monitor
        ViewBag.OpenNotices   = allNotices.Count(n => n.Status == "Open");
        ViewBag.FilledNotices = allNotices.Count(n => n.Status == "Filled");

        var recentPending = allApps
            .Where(a => a.Status == "Pending")
            .OrderByDescending(a => a.CreatedAt)
            .Take(5).ToList();

        return View(recentPending);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GUARD DASHBOARD
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = "Guard")]
    public async Task<IActionResult> Guard()
    {
        var userId = GetUserId();
        var myApps = await _guardService.GetMyApplicationsAsync(userId);

        // MODULE 1: GuardStatus from User entity
        var guardUser = await _userService.GetByIdAsync(userId);
        ViewBag.GuardStatus = guardUser?.GuardStatus ?? "Available";

        // Active ClientRequest assignment
        var allRequests = await _requestService.GetAllRequestsAsync();
        var activeAssignment = allRequests.FirstOrDefault(r =>
            r.AssignedGuardIds != null &&
            r.AssignedGuardIds.Contains(userId) &&
            r.Status is "Assigned" or "Scheduled" or "Active" or "Partially Assigned");
        ViewBag.ActiveAssignment = activeAssignment;

        // Active VIP Mission
        var allVipRequests = await _vipRequestService.GetAllRequestsAsync();
        var vipMission = allVipRequests.FirstOrDefault(r =>
            r.AssignedGuardIds != null &&
            r.AssignedGuardIds.Contains(userId) &&
            r.Status is "Assigned" or "Scheduled" or "Active");
        ViewBag.VipMission = vipMission;

        // Current self-checkin shift
        var activeShift = await _shiftService.GetActiveShiftAsync(userId);
        ViewBag.ActiveShift = activeShift;

        // Today''s assigned shift
        var todayStr = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var myAssignedShifts = await _assignedShiftService.GetGuardScheduleAsync(userId);
        var todaysShift = myAssignedShifts.FirstOrDefault(s => s.ShiftDate == todayStr && s.Status == "Scheduled");
        ViewBag.TodaysShift = todaysShift;

        // Performance rating
        ViewBag.AverageRating = await _ratingService.GetGuardAverageScoreAsync(userId);
        ViewBag.TotalRatings  = (await _ratingService.GetGuardRatingsAsync(userId)).Count;

        // Pending job applications
        ViewBag.PendingJobApplications = myApps
            .Where(a => !string.IsNullOrEmpty(a.JobId) && a.Status == "Pending")
            .ToList();

        return View(myApps);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CLIENT DASHBOARD — MODULE 10: real data replacing empty view
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Client()
    {
        var clientId  = GetUserId();
        var counts    = await _requestService.GetStatusCountsAsync(clientId);
        var requests  = await _requestService.GetRequestsByClientAsync(clientId);

        ViewBag.StatusCounts = counts;

        // Active deployments (need attention or currently running)
        ViewBag.ActiveRequests = requests
            .Where(r => r.Status is "Approved" or "Partially Assigned" or "Assigned" or "Scheduled" or "Active")
            .OrderByDescending(r => r.CreatedAt).ToList();

        // Pending requests awaiting admin approval
        ViewBag.PendingRequests = requests
            .Where(r => r.Status == "Pending")
            .OrderByDescending(r => r.CreatedAt).ToList();

        // Recent history (last 5 completed/rejected/cancelled)
        ViewBag.RecentHistory = requests
            .Where(r => r.Status is "Completed" or "Rejected" or "Cancelled")
            .OrderByDescending(r => r.CreatedAt).Take(5).ToList();

        // Guard candidates waiting for client decision
        var allApps = await _guardService.GetAllApplicationsAsync();
        ViewBag.PendingGuardApplications = allApps
            .Where(a => !string.IsNullOrEmpty(a.JobId) &&
                        a.Status is "Pending" or "Shortlisted" &&
                        requests.Any(r => r.Id == a.JobId))
            .OrderByDescending(a => a.CreatedAt).Take(10).ToList();

        return View();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // VIP CLIENT DASHBOARD
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = "VIP Client")]
    public async Task<IActionResult> Vip()
    {
        var clientId   = GetUserId();
        var allRequests = await _vipRequestService.GetRequestsByClientAsync(clientId);
        var counts      = await _vipRequestService.GetStatusCountsAsync(clientId);

        ViewBag.StatusCounts    = counts;
        ViewBag.ActiveRequests  = allRequests
            .Where(r => r.Status is "Approved" or "Assigned" or "Scheduled" or "Active")
            .OrderByDescending(r => r.CreatedAt).ToList();
        ViewBag.PendingRequests = allRequests.Where(r => r.Status == "Pending").ToList();
        ViewBag.RecentHistory   = allRequests
            .Where(r => r.Status is "Completed" or "Rejected" or "Cancelled")
            .OrderByDescending(r => r.CreatedAt).Take(5).ToList();

        return View();
    }
}
