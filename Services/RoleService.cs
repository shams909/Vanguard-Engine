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

    public async Task<List<Role>> GetAllAsync(int pageNumber, int pageSize)
    {
        return await _unitOfWork.Roles.GetPagedAsync(pageNumber, pageSize);
    }

    public async Task<Role?> GetByIdAsync(string id)
    {
        return await _unitOfWork.Roles.GetByIdAsync(id);
    }

    public async Task<Role> CreateAsync(string roleName, string? description)
    {
        var role = new Role
        {
            RoleName = roleName,
            Description = description
        };

        await _unitOfWork.Roles.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();
        return role;
    }

    public async Task<bool> UpdateAsync(string id, string roleName, string? description)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role is null) return false;

        role.RoleName = roleName;
        role.Description = description;

        _unitOfWork.Roles.Update(role);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role is null) return false;

        _unitOfWork.Roles.Remove(role);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
