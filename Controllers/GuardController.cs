using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vanguard_Engine.Models;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[Authorize]
public class GuardController : Controller
{
    private readonly IGuardApplicationService _guardApplicationService;

    public GuardController(IGuardApplicationService guardApplicationService)
    {
        _guardApplicationService = guardApplicationService;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }

    [HttpGet]
    public IActionResult Apply()
    {
        return View(new GuardApplyViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(GuardApplyViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = GetUserId();
        var success = await _guardApplicationService.ApplyAsync(userId, model.Experience, model.Skills);

        if (success)
        {
            TempData["SuccessMessage"] = "Your application has been submitted successfully.";
            return RedirectToAction(nameof(MyApplications));
        }

        ModelState.AddModelError(string.Empty, "You already have an active application or an error occurred.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> MyApplications()
    {
        var userId = GetUserId();
        var applications = await _guardApplicationService.GetMyApplicationsAsync(userId);
        return View(applications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _guardApplicationService.DeleteAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Application deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Could not delete the application. It might not be in Pending status.";
        }
        return RedirectToAction(nameof(MyApplications));
    }

    // --- ADMIN / RECRUITER ACTIONS ---

    [HttpGet]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<IActionResult> Applications()
    {
        var applications = await _guardApplicationService.GetAllApplicationsAsync();
        return View(applications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<IActionResult> Approve(string id)
    {
        var success = await _guardApplicationService.ApproveAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Application approved.";
        }
        else
        {
            TempData["ErrorMessage"] = "Could not approve the application.";
        }
        return RedirectToAction(nameof(Applications));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Recruiter")]
    public async Task<IActionResult> Reject(string id)
    {
        var success = await _guardApplicationService.RejectAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Application rejected.";
        }
        else
        {
            TempData["ErrorMessage"] = "Could not reject the application.";
        }
        return RedirectToAction(nameof(Applications));
    }
}
