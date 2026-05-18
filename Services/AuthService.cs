using Appwrite;
using Appwrite.Services;
using Appwrite.Enums;
using Microsoft.AspNetCore.Identity;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Models;
using Vanguard_Engine.UnitOfWork;
using System;
using System.Threading.Tasks;

namespace Vanguard_Engine.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppwriteService _appwriteService;
    private readonly IEmailService _emailService;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(IUnitOfWork unitOfWork, IAppwriteService appwriteService, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _appwriteService = appwriteService;
        _emailService = emailService;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<LoginResult> LoginAsync(LoginViewModel model)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(model.Email);
        if (user == null)
        {
            return new LoginResult { Success = false, ErrorMessage = "Invalid email or password" };
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return new LoginResult { Success = false, ErrorMessage = "Invalid email or password" };
        }

        // Email Verification Rule
        if (!user.IsEmailVerified)
        {
            return new LoginResult 
            { 
                Success = false, 
                IsEmailUnverified = true, 
                User = user 
            };
        }

        user.LastLogin = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new LoginResult { Success = true, User = user };
    }

    public async Task<bool> RegisterAsync(RegisterViewModel model, string baseUrl)
    {
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(model.Email);
        if (existingUser != null) return false;

        var roleName = model.UserRole == "Client" ? "Client" : "Guard";
        var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
        
        if (role == null)
        {
            role = new Vanguard_Engine.Entities.Role { RoleName = roleName };
            await _unitOfWork.Roles.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();
        }

        var userId = Guid.NewGuid().ToString("N").Substring(0, 20);
        var verificationToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var tokenExpiry = DateTime.UtcNow.AddHours(24);

        var user = new User
        {
            Id = userId,
            Username = model.Username,
            Email = model.Email,
            Address = model.Address,
            PhoneNumber = model.PhoneNumber,
            RoleId = role.Id,
            LastLogin = DateTime.UtcNow,
            IsEmailVerified = false,
            VerificationToken = verificationToken,
            VerificationTokenExpiry = tokenExpiry
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Send verification email
        var verificationLink = $"{baseUrl.TrimEnd('/')}/auth/verifyemail?userId={userId}&token={verificationToken}";
        await _emailService.SendVerificationEmailAsync(user.Email, user.Username, verificationLink);

        return true;
    }

    public async Task<string> GetOAuth2UrlAsync(string provider, string successUrl, string failureUrl)
    {
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
            // Always fetch the user account via the Users service (with the server's API Key)
            // to avoid client-side CreateSession restrictions on the server-side client
            var usersService = new Users(_appwriteService.GetClient());
            var account = await usersService.Get(userId);

            // Fetch the user from the database collection by Email (fixes Bug 1)
            var user = await _unitOfWork.Users.GetByEmailAsync(account.Email);

            if (user != null)
            {
                // Migrate the user's DB record to use the Google Auth ID if it differs
                if (user.Id != account.Id)
                {
                    // 1. Delete old document with local random C# ID
                    _unitOfWork.Users.Remove(user);
                    
                    // 2. Assign the Google Auth ID
                    user.Id = account.Id;
                    
                    // 3. Create the new document with the updated ID
                    await _unitOfWork.Users.AddAsync(user);
                }

                // Auto-verify OAuth accounts just in case they were registered locally but are logging in via Google
                if (!user.IsEmailVerified)
                {
                    user.IsEmailVerified = true;
                    user.VerificationToken = null;
                    user.VerificationTokenExpiry = null;
                }

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
            Id = model.AppwriteUserId, 
            Username = model.Username,
            Email = model.Email,
            Address = model.Address,
            PhoneNumber = model.PhoneNumber,
            RoleId = role.Id,
            LastLogin = DateTime.UtcNow,
            IsEmailVerified = true // Google accounts should be pre-verified
        };

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

    public async Task<bool> VerifyEmailAsync(string userId, string token)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        if (user.IsEmailVerified) return true;

        if (user.VerificationToken != token) return false;

        if (user.VerificationTokenExpiry == null || user.VerificationTokenExpiry.Value < DateTime.UtcNow)
        {
            return false; // Expired
        }

        user.IsEmailVerified = true;
        user.VerificationToken = null;
        user.VerificationTokenExpiry = null;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ResendVerificationEmailAsync(string email, string baseUrl)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        if (user == null || user.IsEmailVerified) return false;

        var verificationToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var tokenExpiry = DateTime.UtcNow.AddHours(24);

        user.VerificationToken = verificationToken;
        user.VerificationTokenExpiry = tokenExpiry;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        var verificationLink = $"{baseUrl.TrimEnd('/')}/auth/verifyemail?userId={user.Id}&token={verificationToken}";
        await _emailService.SendVerificationEmailAsync(user.Email, user.Username, verificationLink);

        return true;
    }

    public Task LogoutAsync()
    {
        return Task.CompletedTask;
    }

    public async Task<bool> ForgotPasswordAsync(string email, string baseUrl)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        if (user == null) return false;

        // Generate a secure random reset token
        var resetToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var tokenExpiry = DateTime.UtcNow.AddHours(2);

        user.ResetToken = resetToken;
        user.ResetTokenExpiry = tokenExpiry;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        var resetLink = $"{baseUrl.TrimEnd('/')}/auth/resetpassword?token={resetToken}";
        await _emailService.SendPasswordResetEmailAsync(user.Email, user.Username, resetLink);

        return true;
    }

    public async Task<bool> ValidateResetTokenAsync(string token)
    {
        var user = await _unitOfWork.Users.GetByResetTokenAsync(token);
        if (user == null) return false;
        if (user.ResetToken != token) return false;
        if (user.ResetTokenExpiry == null || user.ResetTokenExpiry.Value < DateTime.UtcNow) return false;

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var user = await _unitOfWork.Users.GetByResetTokenAsync(token);
        if (user == null) return false;
        if (user.ResetToken != token) return false;
        if (user.ResetTokenExpiry == null || user.ResetTokenExpiry.Value < DateTime.UtcNow) return false;

        // Hash the new password using the existing hasher
        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);

        // Invalidate the reset token so it cannot be reused
        user.ResetToken = null;
        user.ResetTokenExpiry = null;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
