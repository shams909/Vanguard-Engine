using Microsoft.EntityFrameworkCore;
using Vanguard_Engine.Data;
using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<User?> GetByEmailAsync(string email) =>
        DbSet.FirstOrDefaultAsync(u => u.Email == email);
}
