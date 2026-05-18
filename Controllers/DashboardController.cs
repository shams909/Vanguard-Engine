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

    public DashboardController(
        IUserService userService,
        IGuardApplicationService guardService,
        IHiringService hiringService,
        IClientRequestService requestService)
    {
        _userService = userService;
        _guardService = guardService;
        _hiringService = hiringService;
        _requestService = requestService;
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
        
        // Find general registration profile status
        var generalProfile = myApps.FirstOrDefault(a => string.IsNullOrEmpty(a.JobId) || a.JobId == "");
        ViewBag.GuardStatus = generalProfile?.GuardStatus ?? "Available";

        // Fetch active assignment (ClientRequest where this guard is assigned and status is Approved)
        var allRequests = await _requestService.GetAllRequestsAsync();
        var activeAssignment = allRequests.FirstOrDefault(r => r.Status == "Approved" && r.AssignedGuardIds != null && r.AssignedGuardIds.Contains(userId));
        ViewBag.ActiveAssignment = activeAssignment;

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
    public IActionResult Vip()
    {
        return View();
    }
}
