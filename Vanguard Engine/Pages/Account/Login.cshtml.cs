using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Vanguard_Engine.DTOs.Auth;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public LoginDto Login { get; set; } = new(string.Empty, string.Empty);

    public string? ErrorMessage { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var response = await _authService.LoginAsync(Login);
        if (response == null)
        {
            ErrorMessage = "Invalid email or password";
            return Page();
        }

        TempData["AuthMessage"] = $"Welcome {response.Username}!";
        return RedirectToPage("/Roles/Index");
    }
}
