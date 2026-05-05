using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vanguard_Engine.DTOs.Roles;
using Vanguard_Engine.DTOs.Users;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Pages.Users;

public class CreateModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    public CreateModel(IUserService userService, IRoleService roleService)
    {
        _userService = userService;
        _roleService = roleService;
    }

    [BindProperty]
    public CreateUserDto User { get; set; } = new(string.Empty, string.Empty, string.Empty, null, 0, DateTime.UtcNow);

    public List<RoleDto> Roles { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Roles = await _roleService.GetAllAsync(1, 100);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Roles = await _roleService.GetAllAsync(1, 100);
            return Page();
        }

        await _userService.CreateAsync(User);
        return RedirectToPage("/Users/Index");
    }
}
