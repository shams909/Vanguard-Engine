using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Repositories;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetByNameAsync(string roleName);
}
