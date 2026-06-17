using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class VIPRequestService : IVIPRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    private static readonly Dictionary<string, HashSet<string>> _allowedTransitions = new()
    {
        { "Pending",   new() { "Approved", "Rejected", "Cancelled" } },
        { "Approved",  new() { "Assigned", "Cancelled" } },
        { "Assigned",  new() { "Active",   "Cancelled" } },
        { "Active",    new() { "Completed" } },
        { "Completed", new() { } },
        { "Rejected",  new() { } },
        { "Cancelled", new() { } },
    };

    public VIPRequestService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
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

        // Notify the requesting client
        await _notificationService.CreateNotificationAsync(
            request.VipClientId,
            "VIP Request Submitted",
            $"Your VIP protection request ({request.ProtectionType}) has been submitted and is awaiting review.",
            "Info");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DeleteRequestAsync(string id)
    {
        var existing = await _unitOfWork.VipRequests.GetByIdAsync(id);
        if (existing == null) return (false, "VIP protection request not found.");
        await _unitOfWork.VipRequests.DeleteAsync(id);
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

        // Notify client
        await _notificationService.CreateNotificationAsync(
            existing.VipClientId,
            "VIP Request Approved ✅",
            $"Your VIP protection request ({existing.ProtectionType}) has been approved. Guards will be assigned shortly.",
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

        // Notify client
        await _notificationService.CreateNotificationAsync(
            existing.VipClientId,
            "VIP Request Rejected",
            $"Your VIP protection request ({existing.ProtectionType}) was not approved. Please contact support.",
            "Warning");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> CompleteRequestAsync(string id)
    {
        var existing = await _unitOfWork.VipRequests.GetByIdAsync(id);
        if (existing == null) return (false, "VIP request not found.");

        if (existing.Status != "Active" && existing.Status != "Assigned")
            return (false, $"Only Active or Assigned services can be marked completed. Current status: '{existing.Status}'.");

        await _unitOfWork.VipRequests.UpdateStatusAsync(id, "Completed");
        await ReleaseGuardsAsync(existing.AssignedGuardIds);

        // Notify client
        await _notificationService.CreateNotificationAsync(
            existing.VipClientId,
            "VIP Protection Completed",
            $"Your VIP protection service ({existing.ProtectionType}) has been marked as completed.",
            "Info");

        return (true, string.Empty);
    }

    // ── Phase 3: Elite Guard Assignment ──────────────────────────────────────

    public async Task<List<GuardApplication>> GetEligibleGuardsAsync()
    {
        var allApps = await _unitOfWork.GuardApplications.GetAllAsync();

        var eligibleProfiles = allApps
            .Where(a => string.IsNullOrEmpty(a.JobId) &&
                        a.Status == "Approved" &&
                        a.GuardStatus == "Available")
            .ToList();

        var activeVipRequests = await _unitOfWork.VipRequests.GetAllAsync();
        var busyInVip = activeVipRequests
            .Where(r => r.Status is "Assigned" or "Active")
            .SelectMany(r => r.AssignedGuardIds ?? new List<string>())
            .ToHashSet();

        return eligibleProfiles.Where(g => !busyInVip.Contains(g.UserId)).ToList();
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

        var eligibleGuards = await GetEligibleGuardsAsync();
        var eligibleUserIds = eligibleGuards.Select(g => g.UserId).ToHashSet();

        foreach (var uid in guardUserIds)
        {
            if (!eligibleUserIds.Contains(uid))
                return (false, "One or more selected officers are no longer available. Please refresh the list.");
        }

        await _unitOfWork.VipRequests.UpdateAssignedGuardsAsync(requestId, guardUserIds);
        await _unitOfWork.VipRequests.UpdateStatusAsync(requestId, "Assigned");

        var allApps = await _unitOfWork.GuardApplications.GetAllAsync();
        foreach (var uid in guardUserIds)
        {
            var profile = allApps.FirstOrDefault(a =>
                a.UserId == uid && (string.IsNullOrEmpty(a.JobId) || a.JobId == ""));
            if (profile != null)
                await _unitOfWork.GuardApplications.UpdateGuardStatusAsync(profile.Id, "Busy");

            // Notify each assigned guard
            await _notificationService.CreateNotificationAsync(
                uid,
                "You've Been Assigned to a VIP Mission 🛡️",
                $"You have been assigned to a VIP protection detail ({request.ProtectionType}). Please report for duty.",
                "Info");
        }

        // Notify the requesting client
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

        if (existing.Status != "Assigned")
            return (false, $"Only Assigned services can be started. Current status: '{existing.Status}'.");

        if (existing.AssignedGuardIds == null || !existing.AssignedGuardIds.Any())
            return (false, "Cannot start protection — no officers have been assigned yet.");

        await _unitOfWork.VipRequests.UpdateStatusAsync(id, "Active");

        // Notify the client
        await _notificationService.CreateNotificationAsync(
            existing.VipClientId,
            "Protection Service is Now Active 🟢",
            $"Your VIP protection detail ({existing.ProtectionType}) is now active.",
            "Info");

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

    private async Task ReleaseGuardsAsync(List<string>? guardIds)
    {
        if (guardIds == null || !guardIds.Any()) return;

        var allApps = await _unitOfWork.GuardApplications.GetAllAsync();
        foreach (var uid in guardIds)
        {
            var general = allApps.FirstOrDefault(a =>
                a.UserId == uid && (string.IsNullOrEmpty(a.JobId) || a.JobId == ""));
            if (general != null)
                await _unitOfWork.GuardApplications.UpdateGuardStatusAsync(general.Id, "Available");
        }
    }

    private static Dictionary<string, int> BuildCountMap(List<VIPRequest> requests) => new()
    {
        { "Total",     requests.Count },
        { "Pending",   requests.Count(r => r.Status == "Pending") },
        { "Approved",  requests.Count(r => r.Status == "Approved") },
        { "Assigned",  requests.Count(r => r.Status == "Assigned") },
        { "Active",    requests.Count(r => r.Status == "Active") },
        { "Completed", requests.Count(r => r.Status == "Completed") },
        { "Rejected",  requests.Count(r => r.Status == "Rejected") },
        { "Cancelled", requests.Count(r => r.Status == "Cancelled") },
    };
}
