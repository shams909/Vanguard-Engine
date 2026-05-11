using Vanguard_Engine.DTOs.Roles;
using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<RoleDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        var roles = await _unitOfWork.Roles.GetPagedAsync(pageNumber, pageSize);
        return roles.Select(MapToDto).ToList();
    }

    public async Task<RoleDto?> GetByIdAsync(string id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        return role is null ? null : MapToDto(role);
    }

    public async Task<RoleDto> CreateAsync(CreateRoleDto dto)
    {
        var role = new Role
        {
            RoleName = dto.RoleName,
            Description = dto.Description
        };

        await _unitOfWork.Roles.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(role);
    }

    public async Task<bool> UpdateAsync(string id, UpdateRoleDto dto)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role is null)
        {
            return false;
        }

        role.RoleName = dto.RoleName;
        role.Description = dto.Description;

        _unitOfWork.Roles.Update(role);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role is null)
        {
            return false;
        }

        _unitOfWork.Roles.Remove(role);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static RoleDto MapToDto(Role role) => new(role.Id, role.RoleName, role.Description);
}
