using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}
