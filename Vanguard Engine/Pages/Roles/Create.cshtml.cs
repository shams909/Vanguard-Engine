using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vanguard_Engine.DTOs.Roles;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Pages.Roles;

public class CreateModel : PageModel
{
    private readonly IRoleService _roleService;

    public CreateModel(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [BindProperty]
    public CreateRoleDto Role { get; set; } = new(string.Empty, null);

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _roleService.CreateAsync(Role);
        return RedirectToPage("/Roles/Index");
    }
}
