using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[Authorize(Roles = "Admin")]
public class AnalyticsController : BaseController
{
    private readonly IUserService _userService;
    private readonly IIncidentService _incidentService;
    private readonly IClientRequestService _clientRequestService;
    private readonly IRatingService _ratingService;

    public AnalyticsController(
        IUserService userService, 
        IIncidentService incidentService, 
        IClientRequestService clientRequestService,
        IRatingService ratingService)
    {
        _userService = userService;
        _incidentService = incidentService;
        _clientRequestService = clientRequestService;
        _ratingService = ratingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Fetch aggregates
        var users = await _userService.GetAllAsync(1, 1000);
        var totalGuards = users.Count(u => u.Email.Contains("@")); // Proxy for active guards in prototype
        
        var requests = await _clientRequestService.GetAllRequestsAsync();
        var totalRequests = requests.Count;

        var incidents = await _incidentService.GetAllIncidentsAsync();
        var openIncidents = incidents.Count(i => i.Status == "Open");
        var resolvedIncidents = incidents.Count(i => i.Status == "Resolved");

        var ratings = await _ratingService.GetAllRatingsAsync();
        var averageRating = ratings.Any() ? ratings.Average(r => r.Score) : 0;

        // KPI Data
        ViewBag.TotalGuards = totalGuards;
        ViewBag.TotalRequests = totalRequests;
        ViewBag.TotalIncidents = incidents.Count;
        ViewBag.AverageRating = Math.Round(averageRating, 1);

        // Chart 1: Incident Breakdown (Pie Chart)
        ViewBag.IncidentLabels = new[] { "Open", "Resolved" };
        ViewBag.IncidentData = new[] { openIncidents, resolvedIncidents };

        // Chart 2: Client Requests by Status (Bar Chart)
        var pendingReq = requests.Count(r => r.Status == "Pending");
        var activeReq = requests.Count(r => r.Status == "Active");
        var completedReq = requests.Count(r => r.Status == "Completed");
        
        ViewBag.RequestLabels = new[] { "Pending", "Active", "Completed" };
        ViewBag.RequestData = new[] { pendingReq, activeReq, completedReq };

        // Chart 3: Guard Rating Distribution (Bar Chart)
        var score1 = ratings.Count(r => r.Score == 1);
        var score2 = ratings.Count(r => r.Score == 2);
        var score3 = ratings.Count(r => r.Score == 3);
        var score4 = ratings.Count(r => r.Score == 4);
        var score5 = ratings.Count(r => r.Score == 5);
        
        ViewBag.RatingLabels = new[] { "1 Star", "2 Stars", "3 Stars", "4 Stars", "5 Stars" };
        ViewBag.RatingData = new[] { score1, score2, score3, score4, score5 };

        return View();
    }
}
