using Vanguard_Engine.DTOs.Roles;

namespace Vanguard_Engine.Services;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<RoleDto?> GetByIdAsync(string id);
    Task<RoleDto> CreateAsync(CreateRoleDto dto);
    Task<bool> UpdateAsync(string id, UpdateRoleDto dto);
    Task<bool> DeleteAsync(string id);
}
