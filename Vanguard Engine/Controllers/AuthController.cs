using Microsoft.AspNetCore.Mvc;
using Vanguard_Engine.DTOs.Auth;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var response = await _authService.LoginAsync(dto);
        if (response == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        return Ok(response);
    }
}
