using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Models;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[Authorize]
public class VIPRequestController : Controller
{
    private readonly IVIPRequestService _vipRequestService;

    public VIPRequestController(IVIPRequestService vipRequestService)
    {
        _vipRequestService = vipRequestService;
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    // ═══════════════════════════════════════════════════════════════════════
    // VIP CLIENT ROUTES
    // ═══════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Roles = "VIP Client,VIP")]
    public IActionResult Create() => View(new VIPRequestViewModel());

    [HttpPost]
    [Authorize(Roles = "VIP Client,VIP")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VIPRequestViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var request = new VIPRequest
        {
            VipClientId    = GetUserId(),
            ProtectionType = model.ProtectionType,
            ArmedRequired  = model.ArmedRequired,
            NumberOfGuards = model.NumberOfGuards,
            Duration       = model.Duration,
            Status         = "Pending"
        };

        var result = await _vipRequestService.CreateRequestAsync(request);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(model);
        }

        TempData["Success"] = "Your elite protection request has been filed. Our tactical operations team will review it shortly.";
        return RedirectToAction(nameof(MyRequests));
    }

    [HttpGet]
    [Authorize(Roles = "VIP Client,VIP")]
    public async Task<IActionResult> MyRequests()
    {
        var clientId = GetUserId();
        var requests = await _vipRequestService.GetRequestsByClientAsync(clientId);
        ViewBag.StatusCounts = await _vipRequestService.GetStatusCountsAsync(clientId);
        return View(requests);
    }

    [HttpPost]
    [Authorize(Roles = "VIP Client,VIP")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(string id)
    {
        var request = await _vipRequestService.GetRequestByIdAsync(id);
        if (request == null) return NotFound();

        if (request.VipClientId != GetUserId())
            return RedirectToAction("AccessDenied", "Auth");

        if (request.Status != "Pending")
        {
            TempData["Error"] = "Only pending protection requests can be cancelled.";
            return RedirectToAction(nameof(MyRequests));
        }

        var result = await _vipRequestService.DeleteRequestAsync(id);
        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "Protection request successfully withdrawn." : result.Error;
        return RedirectToAction(nameof(MyRequests));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ADMIN ROUTES — Review & Lifecycle
    // ═══════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<IActionResult> AdminRequests(string? status = null)
    {
        var requests = (!string.IsNullOrWhiteSpace(status) && status != "All")
            ? await _vipRequestService.GetRequestsByStatusAsync(status)
            : await _vipRequestService.GetAllRequestsAsync();

        ViewBag.StatusFilter = status ?? "All";
        ViewBag.StatusCounts = await _vipRequestService.GetAllStatusCountsAsync();
        return View(requests);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Recruiter")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(string id)
    {
        var result = await _vipRequestService.ApproveRequestAsync(id);
        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "VIP request approved." : result.Error;
        return RedirectToAction(nameof(AdminRequests));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Recruiter")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(string id)
    {
        var result = await _vipRequestService.RejectRequestAsync(id);
        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "VIP request rejected." : result.Error;
        return RedirectToAction(nameof(AdminRequests));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Recruiter")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(string id)
    {
        var result = await _vipRequestService.CompleteRequestAsync(id);
        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "VIP service completed. All assigned officers have been released." : result.Error;
        return RedirectToAction(nameof(AdminRequests));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ADMIN ROUTES — Phase 3: Guard Assignment & Activation
    // ═══════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<IActionResult> AssignGuards(string id)
    {
        var request = await _vipRequestService.GetRequestByIdAsync(id);
        if (request == null) return NotFound();

        if (request.Status != "Approved")
        {
            TempData["Error"] = "Guards can only be assigned to Approved requests.";
            return RedirectToAction(nameof(AdminRequests));
        }

        var eligibleGuards = await _vipRequestService.GetEligibleGuardsAsync();

        ViewBag.Request       = request;
        ViewBag.EligibleGuards = eligibleGuards;
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Recruiter")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignGuards(string id, List<string>? guardIds)
    {
        if (guardIds == null || !guardIds.Any())
        {
            TempData["Error"] = "Please select at least one elite officer to assign.";
            return RedirectToAction(nameof(AssignGuards), new { id });
        }

        var result = await _vipRequestService.AssignGuardsAsync(id, guardIds);
        TempData[result.Success ? "Success" : "Error"] =
            result.Success
                ? $"{guardIds.Count} elite officer(s) assigned. Service status is now Assigned."
                : result.Error;

        return RedirectToAction(nameof(AdminRequests));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Recruiter")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartProtection(string id)
    {
        var result = await _vipRequestService.StartProtectionAsync(id);
        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "Elite protection service is now ACTIVE." : result.Error;
        return RedirectToAction(nameof(AdminRequests));
    }
}
