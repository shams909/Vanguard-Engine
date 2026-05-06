using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vanguard_Engine.DTOs.Roles;
using Vanguard_Engine.DTOs.Users;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Pages.Users;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    public IndexModel(IUserService userService, IRoleService roleService)
    {
        _userService = userService;
        _roleService = roleService;
    }

    public List<UserDto> Users { get; private set; } = new();
    public List<RoleDto> Roles { get; private set; } = new();
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }

    public async Task OnGetAsync(int pageNumber = 1, int pageSize = 10)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        Users = await _userService.GetAllAsync(pageNumber, pageSize);
        Roles = await _roleService.GetAllAsync(1, 100);
    }

    public async Task<IActionResult> OnPostUpdateRoleAsync(int userId, int newRoleId)
    {
        var result = await _userService.UpdateRoleAsync(userId, newRoleId);
        if (!result)
        {
            TempData["Error"] = "Failed to update user role.";
        }
        else
        {
            TempData["Success"] = "User role updated successfully.";
        }

        return RedirectToPage(new { pageNumber = PageNumber, pageSize = PageSize });
    }
}
