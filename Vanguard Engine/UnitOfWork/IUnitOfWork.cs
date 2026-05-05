using Vanguard_Engine.Repositories;

namespace Vanguard_Engine.UnitOfWork;

public interface IUnitOfWork
{
    IRoleRepository Roles { get; }
    IUserRepository Users { get; }
    Task<int> SaveChangesAsync();
}
