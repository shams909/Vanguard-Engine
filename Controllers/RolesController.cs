using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vanguard_Engine.Models;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[Authorize(Roles = "Admin")]
public class RolesController : BaseController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
    {
        var model = new RolesIndexViewModel
        {
            Roles = await _roleService.GetAllAsync(pageNumber, pageSize),
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new RolesCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RolesCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        await _roleService.CreateAsync(model.RoleName, model.Description);
        TempData["Success"] = $"Role '{model.RoleName}' created successfully.";
        return RedirectToAction(nameof(Index));
    }
}
