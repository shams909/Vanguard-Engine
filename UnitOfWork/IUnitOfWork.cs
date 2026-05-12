using Vanguard_Engine.Repositories;

namespace Vanguard_Engine.UnitOfWork;

public interface IUnitOfWork
{
    IRoleRepository Roles { get; }
    IUserRepository Users { get; }
    IGuardApplicationRepository GuardApplications { get; }
    Task<int> SaveChangesAsync();
}
