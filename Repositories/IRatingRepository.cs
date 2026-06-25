using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Repositories;

public interface IRatingRepository : IGenericRepository<Rating>
{
    Task<List<Rating>> GetByGuardIdAsync(string guardId);
    Task<List<Rating>> GetByClientIdAsync(string clientId);
}
