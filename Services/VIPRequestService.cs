using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class VIPRequestService : IVIPRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLog;

    private static readonly Dictionary<string, HashSet<string>> _allowedTransitions = new()
    {
        { "Pending",    new() { "Approved", "Rejected", "Cancelled" } },
        { "Approved",   new() { "Assigned",  "Cancelled" } },
        { "Assigned",   new() { "Scheduled", "Cancelled" } },
        { "Scheduled",  new() { "Active",    "Cancelled" } },
        { "Active",     new() { "Completed", "Cancelled" } },
        { "Completed",  new() { } },
        { "Rejected",   new() { } },
        { "Cancelled",  new() { } },
    };

    public VIPRequestService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IAuditLogService auditLog)
    {
        _unitOfWork          = unitOfWork;
        _notificationService = notificationService;
        _auditLog            = auditLog;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<List<VIPRequest>> GetAllRequestsAsync() =>
        await _unitOfWork.VipRequests.GetAllAsync();

    public async Task<List<VIPRequest>> GetRequestsByClientAsync(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId)) return new();
        return await _unitOfWork.VipRequests.GetByClientIdAsync(clientId);
    }

    public async Task<List<VIPRequest>> GetRequestsByStatusAsync(string status)
    {
        if (string.IsNullOrWhiteSpace(status)) return new();
        return await _unitOfWork.VipRequests.GetByStatusAsync(status);
    }

    public async Task<VIPRequest?> GetRequestByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return await _unitOfWork.VipRequests.GetByIdAsync(id);
    }

    // ── VIP Client Operations ────────────────────────────────────────────────

    public async Task<(bool Success, string Error)> CreateRequestAsync(VIPRequest request)
    {
        if (request == null)                                  return (false, "Request payload cannot be empty.");
        if (string.IsNullOrWhiteSpace(request.VipClientId))  return (false, "Client identity is required.");
        if (string.IsNullOrWhiteSpace(request.ProtectionType)) return (false, "Protection type is required.");
        if (request.NumberOfGuards <= 0)                      return (false, "At least one elite officer must be requested.");
        if (request.NumberOfGuards > 20)                      return (false, "Elite tier is capped at 20 officers per deployment.");
        if (string.IsNullOrWhiteSpace(request.Duration))      return (false, "Service duration is required.");

        request.Status = "Pending";
        request.AssignedGuardIds = new List<string>();
        await _unitOfWork.VipRequests.AddAsync(request);

        // MODULE 11: Audit trail
        await _auditLog.LogAsync(
            "VIPRequest", request.Id, "Created",
            request.VipClientId, toValue: "Pending",
            notes: $"{request.ProtectionType}, {request.NumberOfGuards} officer(s)",
            performedByRole: "VIP Client");

        // Notify the requesting client
        await _notificationService.CreateNotificationAsync(
            request.VipClientId,
            "VIP Request Submitted",
            $"Your VIP protection request ({request.ProtectionType}) has been submitted and is awaiting review.",
            "Info");

        // Notify all admins
        await _notificationService.NotifyRoleAsync(
            "Admin",
            "New VIP Protection Request 🛡️",
            $"A VIP client has submitted a new protection request ({request.ProtectionType}). Please review it in the Admin Panel.",
            "Info");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DeleteRequestAsync(string id)
    {
        var existing = await _unitOfWork.VipRequests.GetByIdAsync(id);
        if (existing == null) return (false, "VIP protection request not found.");

        // MODULE 1 FIX: Release guards before deleting so they are not permanently stuck Assigned
        if (existing.AssignedGuardIds?.Any() == true)
            await ReleaseGuardsAsync(existing.AssignedGuardIds);

        await _unitOfWork.VipRequests.DeleteAsync(id);
        return (true, string.Empty);
    }

    /// <summary>
    /// MODULE 5: Proper cancel — sets status to Cancelled, releases guards, and notifies all parties.
    /// Clients can cancel before Assigned. Admins can cancel up to Active.
    /// </summary>
    public async Task<(bool Success, string Error)> CancelRequestAsync(string id, string requesterId, bool isAdmin = false)
    {
        var existing = await _unitOfWork.VipRequests.GetByIdAsync(id);
        if (existing == null) return (false, "VIP request not found.");

        // Ownership check for non-admin clients
        if (!isAdmin && existing.VipClientId != requesterId)
            return (false, "Unauthorized: You can only cancel your own requests.");

        // State guard: clients can cancel up to Assigned; admins can cancel up to Active
        var cancellableByClient = new HashSet<string> { "Pending", "Approved" };
        var cancellableByAdmin  = new HashSet<string> { "Pending", "Approved", "Assigned", "Scheduled", "Active" };
        var allowed = isAdmin ? cancellableByAdmin : cancellableByClient;

        if (!allowed.Contains(existing.Status))
            return (false, $"A request in '{existing.Status}' status cannot be cancelled.");

        await _unitOfWork.VipRequests.UpdateStatusAsync(id, "Cancelled");

        // MODULE 11: Audit trail
        await _auditLog.LogAsync(
            "VIPRequest", id, "Cancelled",
            requesterId, fromValue: existing.Status, toValue: "Cancelled",
            performedByRole: isAdmin ? "Admin" : "VIP Client");

        // Release any assigned guards
        if (existing.AssignedGuardIds?.Any() == true)
            await ReleaseGuardsAsync(existing.AssignedGuardIds);

        // Notify client
        await _notificationService.CreateNotificationAsync(
            existing.VipClientId,
            "VIP Request Cancelled",
            $"Your VIP protection request ({existing.ProtectionType}) has been cancelled.",
            "Warning");

        // Notify assigned guards they are released
        foreach (var guardId in existing.AssignedGuardIds ?? new())
        {
            await _notificationService.CreateNotificationAsync(
                guardId,
                "VIP Mission Cancelled",
                $"The VIP protection detail ({existing.ProtectionType}) you were assigned to has been cancelled. You are now available.",
                "Warning");
        }

        // Notify admins
        await _notificationService.NotifyRoleAsync(
            "Admin",
            "VIP Request Cancelled",
            $"A VIP protection request ({existing.ProtectionType}) has been cancelled.",
            "Warning");

        return (true, string.Empty);
    }

    // ── Admin Workflow ────────────────────────────────────────────────────────

    public async Task<(bool Success, string Error)> UpdateRequestStatusAsync(string id, string status)
    {
        var existing = await _unitOfWork.VipRequests.GetByIdAsync(id);
        if (existing == null) return (false, "VIP request not found.");
        return await TransitionAsync(existing, status);
    }

    public async Task<(bool Success, string Error)> ApproveRequestAsync(string id)
    {
        var existing = await _unitOfWork.VipRequests.GetByIdAsync(id);
        if (existing == null) return (false, "VIP request not found.");
        if (existing.Status != "Pending")
            return (false, $"Only Pending requests can be approved. Current status: '{existing.Status}'.");

        await _unitOfWork.VipRequests.UpdateStatusAsync(id, "Approved");

        // MODULE 11: Audit trail
        await _auditLog.LogAsync("VIPRequest", id, "StatusChanged",
            "system", fromValue: "Pending", toValue: "Approved", performedByRole: "Admin");

        // Notify client
        await _notificationService.CreateNotificationAsync(
            existing.VipClientId,
            "VIP Request Approved ✅",
            $"Your VIP protection request ({existing.ProtectionType}) has been approved. Guards will be assigned shortly.",
            "Info");

        // Notify admins
        await _notificationService.NotifyRoleAsync(
            "Admin",
            "VIP Request Approved",
            $"VIP protection request ({existing.ProtectionType}) has been approved. Please assign guards.",
            "Info");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> RejectRequestAsync(string id)
    {
        var existing = await _unitOfWork.VipRequests.GetByIdAsync(id);
        if (existing == null) return (false, "VIP request not found.");
        if (existing.Status != "Pending")
            return (false, $"Only Pending requests can be rejected. Current status: '{existing.Status}'.");

        await _unitOfWork.VipRequests.UpdateStatusAsync(id, "Rejected");

        // MODULE 11: Audit trail
        await _auditLog.LogAsync("VIPRequest", id, "StatusChanged",
            "system", fromValue: "Pending", toValue: "Rejected", performedByRole: "Admin");

        // Notify client
        await _notificationService.CreateNotificationAsync(
            existing.VipClientId,
            "VIP Request Rejected",
            $"Your VIP protection request ({existing.ProtectionType}) was not approved. Please contact support for more information.",
            "Warning");

        // Notify admins
        await _notificationService.NotifyRoleAsync(
            "Admin",
            "VIP Request Rejected",
            $"A VIP protection request ({existing.ProtectionType}) has been rejected.",
            "Warning");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> CompleteRequestAsync(string id)
    {
        var existing = await _unitOfWork.VipRequests.GetByIdAsync(id);
        if (existing == null) return (false, "VIP request not found.");

        if (existing.Status != "Active" && existing.Status != "Assigned" && existing.Status != "Scheduled")
            return (false, $"Only Active, Scheduled or Assigned services can be marked completed. Current status: '{existing.Status}'.");

        await _unitOfWork.VipRequests.UpdateStatusAsync(id, "Completed");
        await ReleaseGuardsAsync(existing.AssignedGuardIds);

        // MODULE 11: Audit trail
        await _auditLog.LogAsync("VIPRequest", id, "StatusChanged",
            "system", fromValue: existing.Status, toValue: "Completed", performedByRole: "Admin");

        // Notify client
        await _notificationService.CreateNotificationAsync(
            existing.VipClientId,
            "VIP Protection Completed",
            $"Your VIP protection service ({existing.ProtectionType}) has been marked as completed.",
            "Info");

        // Notify each guard they are now available
        foreach (var guardId in existing.AssignedGuardIds ?? new())
        {
            await _notificationService.CreateNotificationAsync(
                guardId,
                "VIP Mission Completed",
                $"Your VIP protection detail ({existing.ProtectionType}) has been completed. You are now available.",
                "Info");
        }

        return (true, string.Empty);
    }

    // ── Phase 3: Elite Guard Assignment ──────────────────────────────────────

    /// <param name="armedRequired">When true, only guards with ArmedLicense = true are returned.</param>
    public async Task<List<GuardApplication>> GetEligibleGuardsAsync(bool armedRequired = false)
    {
        // MODULE 1 FIX: Eligibility determined by User.GuardStatus, not GuardApplication.GuardStatus
        var availableUsers = await _unitOfWork.Users.GetByGuardStatusAsync("Available");
        var availableUserIds = availableUsers.Select(u => u.Id).ToHashSet();

        var allApps = await _unitOfWork.GuardApplications.GetAllAsync();

        // General (onboarding) profiles that are Approved and whose User is Available
        var eligibleProfiles = allApps
            .Where(a => string.IsNullOrEmpty(a.JobId) &&
                        a.Status == "Approved" &&
                        availableUserIds.Contains(a.UserId))
            .ToList();

        // Filter by armed license if the mission requires it
        if (armedRequired)
            eligibleProfiles = eligibleProfiles.Where(g => g.ArmedLicense).ToList();

        return eligibleProfiles;
    }

    public async Task<(bool Success, string Error)> AssignGuardsAsync(string requestId, List<string> guardUserIds)
    {
        if (guardUserIds == null || !guardUserIds.Any())
            return (false, "You must select at least one elite officer to assign.");

        var request = await _unitOfWork.VipRequests.GetByIdAsync(requestId);
        if (request == null) return (false, "VIP request not found.");

        if (request.Status != "Approved")
            return (false, $"Guards can only be assigned to Approved requests. Current status: '{request.Status}'.");

        if (guardUserIds.Count > request.NumberOfGuards)
            return (false, $"You are assigning {guardUserIds.Count} officers but the request only requires {request.NumberOfGuards}.");

        var eligibleGuards = await GetEligibleGuardsAsync(request.ArmedRequired);
        var eligibleUserIds = eligibleGuards.Select(g => g.UserId).ToHashSet();

        foreach (var uid in guardUserIds)
        {
            if (!eligibleUserIds.Contains(uid))
                return (false, "One or more selected officers are no longer available or do not meet armed license requirements. Please refresh.");
        }

        await _unitOfWork.VipRequests.UpdateAssignedGuardsAsync(requestId, guardUserIds);
        await _unitOfWork.VipRequests.UpdateStatusAsync(requestId, "Assigned");

        // MODULE 1 FIX: Set User.GuardStatus = Assigned for each guard — no more GuardApplication status writes
        foreach (var uid in guardUserIds)
        {
            await _unitOfWork.Users.UpdateGuardStatusAsync(uid, "Assigned");

            await _notificationService.CreateNotificationAsync(
                uid,
                "You've Been Assigned to a VIP Mission",
                $"You have been assigned to a VIP protection detail ({request.ProtectionType}). Please report for duty.",
                "Info");
        }

        await _notificationService.CreateNotificationAsync(
            request.VipClientId,
            "Guards Assigned to Your Request",
            $"Elite officers have been assigned to your VIP protection request ({request.ProtectionType}).",
            "Info");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> StartProtectionAsync(string id)
    {
        var existing = await _unitOfWork.VipRequests.GetByIdAsync(id);
        if (existing == null) return (false, "VIP request not found.");

        // Accept both Assigned and Scheduled as valid predecessors to Active
        if (existing.Status != "Assigned" && existing.Status != "Scheduled")
            return (false, $"Only Assigned or Scheduled services can be started. Current status: '{existing.Status}'.");

        if (existing.AssignedGuardIds == null || !existing.AssignedGuardIds.Any())
            return (false, "Cannot start protection — no officers have been assigned yet.");

        await _unitOfWork.VipRequests.UpdateStatusAsync(id, "Active");

        await _notificationService.CreateNotificationAsync(
            existing.VipClientId,
            "Protection Service is Now Active",
            $"Your VIP protection detail ({existing.ProtectionType}) is now active and guards are on duty.",
            "Info");

        foreach (var guardId in existing.AssignedGuardIds)
        {
            await _notificationService.CreateNotificationAsync(
                guardId,
                "VIP Mission Now Active",
                $"VIP protection detail ({existing.ProtectionType}) is now active. You are on duty.",
                "Info");
        }

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ScheduleProtectionAsync(string id)
    {
        var existing = await _unitOfWork.VipRequests.GetByIdAsync(id);
        if (existing == null) return (false, "VIP request not found.");

        if (existing.Status != "Assigned")
            return (false, $"Only Assigned services can be moved to Scheduled. Current status: '{existing.Status}'.");

        await _unitOfWork.VipRequests.UpdateStatusAsync(id, "Scheduled");

        await _notificationService.CreateNotificationAsync(
            existing.VipClientId,
            "VIP Protection Scheduled",
            $"Your VIP protection detail ({existing.ProtectionType}) has been scheduled. Guards will begin at the confirmed time.",
            "Info");

        foreach (var guardId in existing.AssignedGuardIds ?? new())
        {
            await _notificationService.CreateNotificationAsync(
                guardId,
                "VIP Mission Scheduled",
                $"Your VIP protection mission ({existing.ProtectionType}) has been officially scheduled.",
                "Info");
        }

        return (true, string.Empty);
    }

    // ── Dashboard Stats ───────────────────────────────────────────────────────

    public async Task<Dictionary<string, int>> GetStatusCountsAsync(string clientId)
    {
        var requests = await GetRequestsByClientAsync(clientId);
        return BuildCountMap(requests);
    }

    public async Task<Dictionary<string, int>> GetAllStatusCountsAsync()
    {
        var requests = await GetAllRequestsAsync();
        return BuildCountMap(requests);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private async Task<(bool Success, string Error)> TransitionAsync(VIPRequest existing, string targetStatus)
    {
        if (!_allowedTransitions.TryGetValue(existing.Status, out var allowed))
            return (false, $"Unknown current status: '{existing.Status}'.");

        if (!allowed.Contains(targetStatus))
            return (false, $"Invalid transition: '{existing.Status}' → '{targetStatus}'.");

        await _unitOfWork.VipRequests.UpdateStatusAsync(existing.Id, targetStatus);
        return (true, string.Empty);
    }

    /// <summary>
    /// Releases guards back to Available status on the User entity.
    /// MODULE 1 FIX: No longer touches GuardApplication.GuardStatus.
    /// </summary>
    private async Task ReleaseGuardsAsync(List<string>? guardIds)
    {
        if (guardIds == null || !guardIds.Any()) return;

        foreach (var uid in guardIds)
        {
            await _unitOfWork.Users.UpdateGuardStatusAsync(uid, "Available");
        }
    }

    private static Dictionary<string, int> BuildCountMap(List<VIPRequest> requests) => new()
    {
        { "Total",     requests.Count },
        { "Pending",   requests.Count(r => r.Status == "Pending") },
        { "Approved",  requests.Count(r => r.Status == "Approved") },
        { "Assigned",  requests.Count(r => r.Status == "Assigned") },
        { "Scheduled", requests.Count(r => r.Status == "Scheduled") },
        { "Active",    requests.Count(r => r.Status == "Active") },
        { "Completed", requests.Count(r => r.Status == "Completed") },
        { "Rejected",  requests.Count(r => r.Status == "Rejected") },
        { "Cancelled", requests.Count(r => r.Status == "Cancelled") },
    };
}
