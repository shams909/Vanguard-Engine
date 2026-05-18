using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Models;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[Authorize]
public class ClientRequestController : Controller
{
    private readonly IClientRequestService _requestService;
    private readonly IGuardApplicationService _guardService;

    public ClientRequestController(
        IClientRequestService requestService,
        IGuardApplicationService guardService)
    {
        _requestService = requestService;
        _guardService = guardService;
    }

    // ==========================================
    // CLIENT ROUTES
    // ==========================================

    [HttpGet]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> MyRequests()
    {
        var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var myRequests = await _requestService.GetRequestsByClientAsync(clientId);
        
        // Lookup approved guards to resolve names
        var allApps = await _guardService.GetAllApplicationsAsync();
        var guardMap = allApps.ToDictionary(a => a.UserId, a => a.FullName);
        ViewBag.GuardNames = guardMap;

        return View(myRequests);
    }

    [HttpGet]
    [Authorize(Roles = "Client")]
    public IActionResult Create()
    {
        return View(new ClientRequestViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var request = new ClientRequest
        {
            ClientId = clientId,
            NumberOfGuards = model.NumberOfGuards,
            Location = model.Location,
            Duration = model.Duration,
            Status = "Pending"
        };

        var result = await _requestService.CreateRequestAsync(request);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(model);
        }

        return RedirectToAction(nameof(MyRequests));
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(string id)
    {
        var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var request = await _requestService.GetRequestByIdAsync(id);
        if (request == null) return NotFound();

        // Security check: Client can only delete their own request!
        if (request.ClientId != clientId)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }

        if (request.Status != "Pending")
        {
            TempData["Error"] = "Only pending requests can be cancelled.";
            return RedirectToAction(nameof(MyRequests));
        }

        var result = await _requestService.DeleteRequestAsync(id);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Request successfully cancelled!";
        }
        return RedirectToAction(nameof(MyRequests));
    }

    // ==========================================
    // ADMIN / RECRUITER ROUTES
    // ==========================================

    [HttpGet]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<IActionResult> AdminRequests()
    {
        var allRequests = await _requestService.GetAllRequestsAsync();
        
        // Resolve guard names for admin view
        var allApps = await _guardService.GetAllApplicationsAsync();
        var guardMap = allApps.ToDictionary(a => a.UserId, a => a.FullName);
        ViewBag.GuardNames = guardMap;

        return View(allRequests);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<IActionResult> Assign(string id)
    {
        var request = await _requestService.GetRequestByIdAsync(id);
        if (request == null) return NotFound();

        // Get all approved guard applications available for assignment
        var allApps = await _guardService.GetAllApplicationsAsync();
        var approvedGuards = allApps.Where(a => a.Status == "Approved").ToList();

        ViewBag.AvailableGuards = approvedGuards;
        return View(request);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Recruiter")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(string id, List<string> guardIds)
    {
        // Guard IDs must be mapped
        var result = await _requestService.AssignGuardsToRequestAsync(id, guardIds ?? new List<string>());
        
        // Auto-approve status on successful assignment
        if (result.Success && guardIds != null && guardIds.Any())
        {
            await _requestService.UpdateRequestStatusAsync(id, "Approved");
        }
        else if (result.Success)
        {
            // Reset to pending if no guards assigned
            await _requestService.UpdateRequestStatusAsync(id, "Pending");
        }

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Guards assigned successfully!";
        }
        return RedirectToAction(nameof(AdminRequests));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Recruiter")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string id, string status)
    {
        var result = await _requestService.UpdateRequestStatusAsync(id, status);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = $"Request status updated to '{status}'!";
        }
        return RedirectToAction(nameof(AdminRequests));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Recruiter")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _requestService.DeleteRequestAsync(id);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Deployment request successfully deleted!";
        }
        return RedirectToAction(nameof(AdminRequests));
    }
}
