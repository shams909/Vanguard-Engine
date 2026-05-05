using BCrypt.Net;
using Vanguard_Engine.DTOs.Users;
using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<UserDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        var users = await _unitOfWork.Users.GetPagedAsync(pageNumber, pageSize);
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Address = dto.Address,
            RoleId = dto.RoleId,
            LastLogin = dto.LastLogin
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserDto dto)
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
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
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
