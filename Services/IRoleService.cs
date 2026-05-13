using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IRoleService
{
    Task<List<Role>> GetAllAsync(int pageNumber, int pageSize);
    Task<Role?> GetByIdAsync(string id);
    Task<Role> CreateAsync(string roleName, string? description);
    Task<bool> UpdateAsync(string id, string roleName, string? description);
    Task<bool> DeleteAsync(string id);
}
