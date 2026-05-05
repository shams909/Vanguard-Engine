using Microsoft.EntityFrameworkCore;
using Vanguard_Engine.Data;
using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<Role?> GetByNameAsync(string roleName) =>
        DbSet.FirstOrDefaultAsync(r => r.RoleName == roleName);
}
