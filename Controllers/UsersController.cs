using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vanguard_Engine.Models;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    public UsersController(IUserService userService, IRoleService roleService)
    {
        _userService = userService;
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
    {
        var model = new UsersIndexViewModel
        {
            Users = await _userService.GetAllAsync(pageNumber, pageSize),
            Roles = await _roleService.GetAllAsync(1, 100),
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRole(string userId, string newRoleId, int pageNumber = 1, int pageSize = 10)
    {
        var result = await _userService.UpdateRoleAsync(userId, newRoleId);
        TempData[result ? "Success" : "Error"] = result
            ? "User role updated successfully."
            : "Failed to update user role.";
        return RedirectToAction(nameof(Index), new { pageNumber, pageSize });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new UsersCreateViewModel
        {
            Roles = await _roleService.GetAllAsync(1, 100)
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsersCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Roles = await _roleService.GetAllAsync(1, 100);
            return View(model);
        }

        await _userService.CreateAsync(model.Username, model.Email, model.Password, model.Address, model.RoleId);
        TempData["Success"] = $"User '{model.Username}' created successfully.";
        return RedirectToAction(nameof(Index));
    }
}
