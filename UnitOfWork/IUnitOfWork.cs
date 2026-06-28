using Vanguard_Engine.Repositories;

namespace Vanguard_Engine.UnitOfWork;

public interface IUnitOfWork
{
    IRoleRepository             Roles              { get; }
    INotificationRepository     Notifications      { get; }
    IUserRepository             Users              { get; }
    IGuardApplicationRepository GuardApplications  { get; }
    IHiringNoticeRepository     HiringNotices      { get; }
    IClientRequestRepository    ClientRequests     { get; }
    IVIPRequestRepository       VipRequests        { get; }
    IGuardShiftRepository       GuardShifts        { get; }
    IAssignedShiftRepository    AssignedShifts     { get; }
    IIncidentRepository         Incidents          { get; }
    IRatingRepository           Ratings            { get; }
    IVipApplicationRepository   VipApplications    { get; }

    /// <summary>MODULE 11: Immutable audit trail for all state transitions.</summary>
    IAuditLogRepository         AuditLogs          { get; }

    Task<int> SaveChangesAsync();
}

