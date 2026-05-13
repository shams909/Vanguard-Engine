using Vanguard_Engine.Repositories;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.UnitOfWork;

public class AppwriteUnitOfWork : IUnitOfWork
{
    public IRoleRepository Roles { get; }
    public IUserRepository Users { get; }
    public IGuardApplicationRepository GuardApplications { get; }
    public IHiringNoticeRepository HiringNotices { get; }

    public AppwriteUnitOfWork(IAppwriteService appwriteService)
    {
        Roles = new AppwriteRoleRepository(appwriteService);
        Users = new AppwriteUserRepository(appwriteService);
        GuardApplications = new GuardApplicationRepository(appwriteService);
        HiringNotices = new HiringNoticeRepository(appwriteService);
    }

    public Task<int> SaveChangesAsync()
    {
        // Appwrite operations are atomic at the document level.
        // We don't have a multi-collection transaction in the standard SDK easily.
        // Returning 1 to simulate success.
        return Task.FromResult(1);
    }
}
