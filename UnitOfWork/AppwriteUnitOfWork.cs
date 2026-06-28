using Vanguard_Engine.Repositories;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.UnitOfWork;

public class AppwriteUnitOfWork : IUnitOfWork
{
    public IRoleRepository             Roles              { get; }
    public INotificationRepository     Notifications      { get; }
    public IUserRepository             Users              { get; }
    public IGuardApplicationRepository GuardApplications  { get; }
    public IHiringNoticeRepository     HiringNotices      { get; }
    public IClientRequestRepository    ClientRequests     { get; }
    public IVIPRequestRepository       VipRequests        { get; }
    public IGuardShiftRepository       GuardShifts        { get; }
    public IAssignedShiftRepository    AssignedShifts     { get; }
    public IIncidentRepository         Incidents          { get; }
    public IRatingRepository           Ratings            { get; }
    public IVipApplicationRepository   VipApplications    { get; }
    public IAuditLogRepository         AuditLogs          { get; }

    public AppwriteUnitOfWork(IAppwriteService appwriteService)
    {
        Roles             = new AppwriteRoleRepository(appwriteService);
        Users             = new AppwriteUserRepository(appwriteService);
        GuardApplications = new GuardApplicationRepository(appwriteService);
        HiringNotices     = new HiringNoticeRepository(appwriteService);
        ClientRequests    = new ClientRequestRepository(appwriteService);
        VipRequests       = new VIPRequestRepository(appwriteService);
        GuardShifts       = new GuardShiftRepository(appwriteService);
        AssignedShifts    = new AppwriteAssignedShiftRepository(appwriteService);
        Incidents         = new AppwriteIncidentRepository(appwriteService);
        Ratings           = new AppwriteRatingRepository(appwriteService);
        VipApplications   = new VipApplicationRepository(appwriteService);
        Notifications     = new NotificationRepository(appwriteService);
        AuditLogs         = new AppwriteAuditLogRepository(appwriteService);
    }

    public Task<int> SaveChangesAsync()
    {
        // Appwrite operations are atomic at the document level.
        // We don't have a multi-collection transaction in the standard SDK easily.
        // Returning 1 to simulate success.
        return Task.FromResult(1);
    }
}
