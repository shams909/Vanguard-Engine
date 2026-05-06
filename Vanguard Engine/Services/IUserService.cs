using Vanguard_Engine.DTOs.Users;

namespace Vanguard_Engine.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<bool> UpdateAsync(int id, UpdateUserDto dto);
    Task<bool> UpdateRoleAsync(int userId, int newRoleId);
    Task<bool> DeleteAsync(int id);
}
