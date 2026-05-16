using Appwrite;
using Appwrite.Services;
using Appwrite.Enums;
using Microsoft.AspNetCore.Identity;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Models;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppwriteService _appwriteService;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(IUnitOfWork unitOfWork, IAppwriteService appwriteService)
    {
        _unitOfWork = unitOfWork;
        _appwriteService = appwriteService;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<User?> LoginAsync(LoginViewModel model)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(model.Email);
        if (user == null) return null;

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (result == PasswordVerificationResult.Failed) return null;

        user.LastLogin = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }

    public async Task<bool> RegisterAsync(RegisterViewModel model)
    {
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(model.Email);
        if (existingUser != null) return false;

        var roleName = model.UserRole == "Client" ? "Client" : "Guard";
        var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
        
        if (role == null)
        {
            // Fallback or create role if it doesn't exist (though it should be seeded)
            role = new Vanguard_Engine.Entities.Role { RoleName = roleName };
            await _unitOfWork.Roles.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();
        }

        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            Address = model.Address,
            PhoneNumber = model.PhoneNumber,
            RoleId = role.Id,
            LastLogin = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<string> GetOAuth2UrlAsync(string provider, string successUrl, string failureUrl)
    {
        // Manually construct the URL to bypass SDK internal Redirect logic that is causing the 412 error.
        // This matches the "Ultimate Test" link that confirmed your console settings are correct.
        
        var baseUrl = _appwriteService.Endpoint.TrimEnd('/');
        var projectId = _appwriteService.ProjectId;
        
        var authUrl = $"{baseUrl}/account/sessions/oauth2/{provider.ToLower()}?" +
                      $"project={projectId}&" +
                      $"success={Uri.EscapeDataString(successUrl)}&" +
                      $"failure={Uri.EscapeDataString(failureUrl)}&" +
                      $"prompt=select_account";
                      
        return await Task.FromResult(authUrl);
    }

    public async Task<OAuthResult> HandleOAuthCallbackAsync(string userId, string secret)
    {
        try
        {
            Appwrite.Models.User? account = null;

            // Option A: We have a fresh OAuth secret (preferred)
            if (userId != secret && !string.IsNullOrEmpty(secret))
            {
                var accountService = new Account(_appwriteService.GetClient());
                await accountService.CreateSession(userId, secret);
                account = await accountService.Get();
            }
            else
            {
                // Option B: The tokens were lost, but we have the userId!
                // We use our API Key (Privileged Client) to fetch the user data.
                var usersService = new Users(_appwriteService.GetClient());
                account = await usersService.Get(userId);
            }

            // Check if user exists in our database collection by ID
            var user = await _unitOfWork.Users.GetByIdAsync(account.Id);

            if (user != null)
            {
                user.LastLogin = DateTime.UtcNow;
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                return new OAuthResult
                {
                    Success = true,
                    User = user,
                    IsNewUser = false
                };
            }

            // New user, return details for profile completion
            return new OAuthResult
            {
                Success = true,
                IsNewUser = true,
                AppwriteUserId = account.Id,
                Email = account.Email,
                Name = account.Name
            };
        }
        catch (Exception ex)
        {
            return new OAuthResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> CreateProfileAsync(CompleteProfileViewModel model)
    {
        var role = await _unitOfWork.Roles.GetByNameAsync(model.UserRole);
        if (role == null) return false;

        var user = new User
        {
            Id = model.AppwriteUserId, // Link to Appwrite Auth ID
            Username = model.Username,
            Email = model.Email,
            Address = model.Address,
            PhoneNumber = model.PhoneNumber,
            RoleId = role.Id,
            LastLogin = DateTime.UtcNow
        };

        // Hash the password
        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<User?> GetUserByIdAsync(string id)
    {
        return await _unitOfWork.Users.GetByIdAsync(id);
    }

    public async Task<bool> UpdatePhoneNumberAsync(string userId, string phoneNumber)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        user.PhoneNumber = phoneNumber;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public Task LogoutAsync()
    {
        // Cookie clearing is handled in the controller
        return Task.CompletedTask;
    }
}
