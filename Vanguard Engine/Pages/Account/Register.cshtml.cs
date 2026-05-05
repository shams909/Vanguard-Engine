using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vanguard_Engine.DTOs.Auth;
using Vanguard_Engine.DTOs.Roles;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly IRoleService _roleService;

    public RegisterModel(IAuthService authService, IRoleService roleService)
    {
        _authService = authService;
        _roleService = roleService;
    }

    [BindProperty]
    public RegisterDto Register { get; set; } = new(string.Empty, string.Empty, string.Empty, null, 0);

    public List<RoleDto> Roles { get; private set; } = new();
    public string? ErrorMessage { get; private set; }

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

        var response = await _authService.RegisterAsync(Register);
        if (!response.Success)
        {
            ErrorMessage = response.Message;
            Roles = await _roleService.GetAllAsync(1, 100);
            return Page();
        }

        TempData["AuthMessage"] = response.Message;
        return RedirectToPage("/Account/Login");
    }
}
