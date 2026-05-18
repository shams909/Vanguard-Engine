using System.Threading.Tasks;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Models;

namespace Vanguard_Engine.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterViewModel model, string baseUrl);
    Task<LoginResult> LoginAsync(LoginViewModel model);
    Task<string> GetOAuth2UrlAsync(string provider, string successUrl, string failureUrl);
    Task<OAuthResult> HandleOAuthCallbackAsync(string userId, string secret);
    Task<bool> CreateProfileAsync(CompleteProfileViewModel model);
    Task<User?> GetUserByIdAsync(string id);
    Task<bool> UpdatePhoneNumberAsync(string userId, string phoneNumber);
    Task<bool> VerifyEmailAsync(string userId, string token);
    Task<bool> ResendVerificationEmailAsync(string email, string baseUrl);
    Task<bool> ForgotPasswordAsync(string email, string baseUrl);
    Task<bool> ValidateResetOtpAsync(string email, string otp);
    Task<bool> ResetPasswordAsync(string email, string otp, string newPassword);
    Task LogoutAsync();
}
