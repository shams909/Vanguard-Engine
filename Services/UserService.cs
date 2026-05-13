using Microsoft.AspNetCore.Identity;
using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PasswordHasher<User> _passwordHasher;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<List<User>> GetAllAsync(int pageNumber, int pageSize)
    {
        return await _unitOfWork.Users.GetPagedAsync(pageNumber, pageSize);
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _unitOfWork.Users.GetByIdAsync(id);
    }

    public async Task<User> CreateAsync(string username, string email, string password, string? address, string? roleId)
    {
        var user = new User
        {
            Username = username.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Address = address,
            RoleId = roleId,
            LastLogin = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UpdateAsync(string id, string username, string email, string? password, string? address, string? roleId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user is null) return false;

        user.Email = email.Trim().ToLowerInvariant();
        user.Username = username.Trim();
        user.Address = address;
        user.RoleId = roleId;

        if (!string.IsNullOrWhiteSpace(password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateRoleAsync(string userId, string newRoleId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null) return false;

        var role = await _unitOfWork.Roles.GetByIdAsync(newRoleId);
        if (role is null) return false;

        user.RoleId = newRoleId;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user is null) return false;

        _unitOfWork.Users.Remove(user);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
