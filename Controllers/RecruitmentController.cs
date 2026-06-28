using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Models;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

public class RecruitmentController : BaseController
{
    private readonly IHiringService _hiringService;

    public RecruitmentController(IHiringService hiringService)
    {
        _hiringService = hiringService;
    }

    // ── PUBLIC: VIEW OPEN JOBS ─────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var notices = await _hiringService.GetOpenNoticesAsync();
        return View(notices);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var notice = await _hiringService.GetByIdAsync(id);
        if (notice == null) return NotFound();
        return View(notice);
    }

    // ── ADMIN / RECRUITER: MANAGE JOBS ────────────────────────────────────────
    [Authorize(Roles = "Admin,Recruiter")]
    [HttpGet]
    public async Task<IActionResult> Manage()
    {
        var notices = await _hiringService.GetAllAsync();
        return View(notices);
    }

    [Authorize(Roles = "Admin,Recruiter")]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new HiringNoticeViewModel());
    }

    [Authorize(Roles = "Admin,Recruiter")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HiringNoticeViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var notice = new HiringNotice
        {
            Title           = model.Title,
            JobType         = model.JobType,
            Priority        = model.Priority,
            Description     = model.Description,
            Requirements    = model.Requirements,
            Location        = model.Location,
            SalaryRange     = model.SalaryRange,
            ExpiryDate      = model.ExpiryDate,
            NumberOfPositions = model.NumberOfPositions,
            PostedByUserId  = GetUserId()
        };

        var (success, error) = await _hiringService.CreateAsync(notice);
        if (success)
        {
            SetSuccess("Hiring notice posted successfully.");
            return RedirectToAction(nameof(Manage));
        }

        ModelState.AddModelError(string.Empty, error);
        return View(model);
    }

    [Authorize(Roles = "Admin,Recruiter")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(string id)
    {
        var (success, error) = await _hiringService.CloseNoticeAsync(id);
        SetResult((success, error), "Job post closed.");
        return RedirectToAction(nameof(Manage));
    }

    [Authorize(Roles = "Admin,Recruiter")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var (success, error) = await _hiringService.DeleteAsync(id);
        SetResult((success, error), "Job post deleted.");
        return RedirectToAction(nameof(Manage));
    }
}

