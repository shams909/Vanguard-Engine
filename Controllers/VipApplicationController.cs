using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[Authorize]
public class VipApplicationController : BaseController
{
    private readonly IVipApplicationService _vipApplicationService;

    public VipApplicationController(IVipApplicationService vipApplicationService)
    {
        _vipApplicationService = vipApplicationService;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // CLIENT — Apply for VIP
    // ═══════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Apply()
    {
        var clientId = GetUserId();
        var pendingApp = await _vipApplicationService.GetMyPendingApplicationAsync(clientId);
        
        if (pendingApp != null)
        {
            return RedirectToAction(nameof(MyApplication));
        }

        return View(new VipApplication());
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(VipApplication model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var clientId = GetUserId();
        model.ClientId = clientId;
        model.ClientName = GetUserName();

        var result = await _vipApplicationService.ApplyAsync(model);

        if (result.Success)
        {
            TempData["SuccessMessage"] = "Your VIP application has been submitted and is under review.";
            return RedirectToAction(nameof(MyApplication));
        }

        TempData["ErrorMessage"] = result.Error;
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> MyApplication()
    {
        var clientId = GetUserId();
        var apps = await _vipApplicationService.GetMyApplicationsAsync(clientId);
        return View(apps.FirstOrDefault()); // Show the most recent application
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ADMIN — Review Applications
    // ═══════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminPanel()
    {
        var pending = await _vipApplicationService.GetPendingApplicationsAsync();
        return View(pending);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(string applicationId)
    {
        var result = await _vipApplicationService.ApproveAsync(applicationId);
        
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = 
            result.Success ? "VIP Application approved. Client has been upgraded." : result.Error;

        return RedirectToAction(nameof(AdminPanel));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(string applicationId)
    {
        var result = await _vipApplicationService.RejectAsync(applicationId);

        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = 
            result.Success ? "VIP Application rejected." : result.Error;

        return RedirectToAction(nameof(AdminPanel));
    }
}
