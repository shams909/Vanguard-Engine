using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vanguard_Engine.Entities;
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

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    // ── USER: APPLY ──────────────────────────────

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

        var application = new GuardApplication
        {
            FullName = model.FullName.Trim(),
            Phone = model.Phone.Trim(),
            NationalId = model.NationalId.Trim(),
            Address = model.Address.Trim(),
            YearsOfExperience = model.YearsOfExperience,
            Experience = model.Experience.Trim(),
            Skills = model.Skills.Trim(),
            PreferredLocation = model.PreferredLocation.Trim(),
            ArmedLicense = model.ArmedLicense
        };

        var (success, error) = await _guardApplicationService.ApplyAsync(GetUserId(), application);

        if (success)
        {
            TempData["SuccessMessage"] = "Your application has been submitted successfully. Our team will review it shortly.";
            return RedirectToAction(nameof(MyApplications));
        }

        ModelState.AddModelError(string.Empty, error);
        return View(model);
    }

    // ── USER: MY APPLICATIONS ────────────────────

    [HttpGet]
    public async Task<IActionResult> MyApplications()
    {
        var applications = await _guardApplicationService.GetMyApplicationsAsync(GetUserId());
        return View(applications);
    }

    // ── USER: APPLICATION DETAIL ─────────────────

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var application = await _guardApplicationService.GetApplicationByIdAsync(id, GetUserId());
        if (application == null) return NotFound();
        return View(application);
    }

    // ── USER: DELETE ─────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var (success, error) = await _guardApplicationService.DeleteAsync(id, GetUserId());

        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success ? "Application withdrawn successfully." : error;

        return RedirectToAction(nameof(MyApplications));
    }

    // ── ADMIN / RECRUITER: ALL APPLICATIONS ──────

    [HttpGet]
    [Authorize(Roles = "Admin,Recruiter,Client")]
    public async Task<IActionResult> Applications()
    {
        var applications = await _guardApplicationService.GetAllApplicationsAsync();
        return View(applications);
    }

    // ── ADMIN / RECRUITER: APPROVE ───────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Recruiter,Client")]
    public async Task<IActionResult> Approve(string id)
    {
        var (success, error) = await _guardApplicationService.ApproveAsync(id);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success ? "Application approved successfully." : error;

        return RedirectToAction(nameof(Applications));
    }

    // ── ADMIN / RECRUITER: REJECT ─────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Recruiter,Client")]
    public async Task<IActionResult> Reject(string id)
    {
        var (success, error) = await _guardApplicationService.RejectAsync(id);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] =
            success ? "Application rejected." : error;

        return RedirectToAction(nameof(Applications));
    }
}
