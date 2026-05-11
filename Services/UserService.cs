using Microsoft.AspNetCore.Identity;
using Vanguard_Engine.DTOs.Users;
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

    public async Task<List<UserDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        var users = await _unitOfWork.Users.GetPagedAsync(pageNumber, pageSize);
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(string id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var user = new User
        {
            Username = dto.Username.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            Address = dto.Address,
            RoleId = dto.RoleId,
            LastLogin = dto.LastLogin
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<bool> UpdateAsync(string id, UpdateUserDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user is null)
        {
            return false;
        }

        user.Email = dto.Email.Trim().ToLowerInvariant();
        user.Username = dto.Username.Trim();
        user.Address = dto.Address;
        user.RoleId = dto.RoleId;
        user.LastLogin = dto.LastLogin;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
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
        if (user is null)
        {
            return false;
        }

        _unitOfWork.Users.Remove(user);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static UserDto MapToDto(User user) => new(user.Id, user.Username, user.Email, user.Address, user.RoleId, user.LastLogin);
}
