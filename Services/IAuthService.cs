using Vanguard_Engine.Entities;
using Vanguard_Engine.Models;

namespace Vanguard_Engine.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterViewModel model);
    Task<User?> LoginAsync(LoginViewModel model);
    Task LogoutAsync();
}
