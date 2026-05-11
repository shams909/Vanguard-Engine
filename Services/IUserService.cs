using Vanguard_Engine.DTOs.Users;

namespace Vanguard_Engine.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<UserDto?> GetByIdAsync(string id);
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<bool> UpdateAsync(string id, UpdateUserDto dto);
    Task<bool> UpdateRoleAsync(string userId, string newRoleId);
    Task<bool> DeleteAsync(string id);
}
