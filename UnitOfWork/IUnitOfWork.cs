using Vanguard_Engine.Repositories;

namespace Vanguard_Engine.UnitOfWork;

public interface IUnitOfWork
{
    IRoleRepository Roles { get; }
    IUserRepository Users { get; }
    IGuardApplicationRepository GuardApplications { get; }
    IHiringNoticeRepository HiringNotices { get; }
    IClientRequestRepository ClientRequests { get; }
    Task<int> SaveChangesAsync();
}
