using Vanguard_Engine.Entities;
using Vanguard_Engine.Models;

namespace Vanguard_Engine.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterViewModel model);
    Task<User?> LoginAsync(LoginViewModel model);
    Task<string> GetOAuth2UrlAsync(string provider, string successUrl, string failureUrl);
    Task<OAuthResult> HandleOAuthCallbackAsync(string userId, string secret);
    Task<bool> CreateProfileAsync(CompleteProfileViewModel model);
    Task<User?> GetUserByIdAsync(string id);
    Task<bool> UpdatePhoneNumberAsync(string userId, string phoneNumber);
    Task LogoutAsync();
}
