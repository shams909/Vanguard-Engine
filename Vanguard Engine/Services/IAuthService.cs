using Vanguard_Engine.DTOs.Auth;

namespace Vanguard_Engine.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
}
