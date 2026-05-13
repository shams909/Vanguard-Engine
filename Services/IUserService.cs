using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IUserService
{
    Task<List<User>> GetAllAsync(int pageNumber, int pageSize);
    Task<User?> GetByIdAsync(string id);
    Task<User> CreateAsync(string username, string email, string password, string? address, string? roleId);
    Task<bool> UpdateAsync(string id, string username, string email, string? password, string? address, string? roleId);
    Task<bool> UpdateRoleAsync(string userId, string newRoleId);
    Task<bool> DeleteAsync(string id);
}
