using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByResetTokenAsync(string token);

    /// <summary>Updates only the guardStatus field for a user — efficient single-field patch.</summary>
    Task UpdateGuardStatusAsync(string userId, string status);

    /// <summary>Returns all users whose guardStatus matches the given value. Used for dashboard KPIs.</summary>
    Task<List<User>> GetByGuardStatusAsync(string status);
}

