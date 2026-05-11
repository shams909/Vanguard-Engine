using Microsoft.AspNetCore.Identity;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Models;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
            role = new Role { RoleName = roleName };
            await _unitOfWork.Roles.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();
        }

        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            Address = model.Address,
            RoleId = role.Id,
            LastLogin = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public Task LogoutAsync()
    {
        // Cookie clearing is handled in the controller
        return Task.CompletedTask;
    }
}
