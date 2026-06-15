using Vanguard_Engine.Repositories;

namespace Vanguard_Engine.UnitOfWork;

public interface IUnitOfWork
{
    IRoleRepository Roles { get; }
    INotificationRepository Notifications { get; }
    IUserRepository Users { get; }
    IGuardApplicationRepository GuardApplications { get; }
    IHiringNoticeRepository HiringNotices { get; }
    IClientRequestRepository ClientRequests { get; }
    IVIPRequestRepository VipRequests { get; }
    IGuardShiftRepository GuardShifts { get; }
    IVipApplicationRepository VipApplications { get; }
    Task<int> SaveChangesAsync();
}
