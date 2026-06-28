namespace Vanguard_Engine.Helpers;

/// <summary>
/// MODULE 3: Central source of truth for all status strings used across the system.
/// All services, controllers, and views must reference these constants instead of
/// hardcoding magic strings, preventing typos and state machine drift.
/// </summary>
public static class StatusConstants
{
    // Guard Operational Status (User.GuardStatus)
    public static class GuardStatus
    {
        public const string Available   = "Available";
        public const string Assigned    = "Assigned";
        public const string OnDuty      = "OnDuty";
        public const string Unavailable = "Unavailable";
        public const string Suspended   = "Suspended";

        public static readonly IReadOnlyList<string> All =
            new[] { Available, Assigned, OnDuty, Unavailable, Suspended };

        public static bool IsDeployable(string? status) => status == Available;
        public static bool IsActivelyWorking(string? status) => status is Assigned or OnDuty;
    }

    // ClientRequest Lifecycle
    public static class ClientRequest
    {
        public const string Pending           = "Pending";
        public const string Approved          = "Approved";
        public const string PartiallyAssigned = "Partially Assigned";
        public const string Assigned          = "Assigned";
        public const string Scheduled         = "Scheduled";
        public const string Active            = "Active";
        public const string Completed         = "Completed";
        public const string Rejected          = "Rejected";
        public const string Cancelled         = "Cancelled";

        public static bool IsOpen(string? status) => status is Approved or PartiallyAssigned;
        public static bool IsTerminal(string? status) => status is Completed or Rejected or Cancelled;
    }

    // VIPRequest Lifecycle
    public static class VipRequest
    {
        public const string Pending   = "Pending";
        public const string Approved  = "Approved";
        public const string Assigned  = "Assigned";
        public const string Scheduled = "Scheduled";
        public const string Active    = "Active";
        public const string Completed = "Completed";
        public const string Rejected  = "Rejected";
        public const string Cancelled = "Cancelled";

        public static bool IsActiveOrInProgress(string? status) => status is Assigned or Scheduled or Active;
        public static bool IsTerminal(string? status) => status is Completed or Rejected or Cancelled;
    }

    // Guard Application States
    public static class GuardApplication
    {
        public const string Pending     = "Pending";
        public const string Shortlisted = "Shortlisted";
        public const string Approved    = "Approved";
        public const string Accepted    = "Accepted";
        public const string Rejected    = "Rejected";
    }

    // Hiring Notice States
    public static class HiringNotice
    {
        public const string Open   = "Open";
        public const string Closed = "Closed";
        public const string Filled = "Filled";
    }

    // Shift States
    public static class Shift
    {
        public const string Scheduled = "Scheduled";
        public const string Active    = "Active";
        public const string Completed = "Completed";
        public const string Missed    = "Missed";
    }

    // Incident / Complaint States
    public static class Incident
    {
        public const string Open     = "Open";
        public const string Resolved = "Resolved";
    }

    // Notification Types
    public static class NotificationType
    {
        public const string Info    = "Info";
        public const string Warning = "Warning";
        public const string Error   = "Error";
        public const string Success = "Success";
    }

    // System Roles
    public static class Role
    {
        public const string Admin     = "Admin";
        public const string Recruiter = "Recruiter";
        public const string Guard     = "Guard";
        public const string Client    = "Client";
        public const string VipClient = "VIP Client";

        public static readonly IReadOnlyList<string> All =
            new[] { Admin, Recruiter, Guard, Client, VipClient };
    }
}
