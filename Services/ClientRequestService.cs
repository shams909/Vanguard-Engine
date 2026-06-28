using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class ClientRequestService : IClientRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLog;

    // MODULE 4: State machine — defines every legal transition
    private static readonly Dictionary<string, HashSet<string>> _allowedTransitions = new()
    {
        { "Pending",           new() { "Approved", "Rejected", "Cancelled" } },
        { "Approved",          new() { "Partially Assigned", "Assigned", "Cancelled", "Rejected" } },
        { "Partially Assigned",new() { "Partially Assigned", "Assigned", "Cancelled" } },
        { "Assigned",          new() { "Partially Assigned", "Assigned", "Scheduled", "Cancelled" } },
        { "Scheduled",         new() { "Active", "Cancelled" } },
        { "Active",            new() { "Completed" } },
        { "Completed",         new() { } },
        { "Rejected",          new() { } },
        { "Cancelled",         new() { } },
    };

    public ClientRequestService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IAuditLogService auditLog)
    {
        _unitOfWork          = unitOfWork;
        _notificationService = notificationService;
        _auditLog            = auditLog;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<List<ClientRequest>> GetAllRequestsAsync()
        => await _unitOfWork.ClientRequests.GetAllAsync();

    public async Task<List<ClientRequest>> GetRequestsByClientAsync(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId)) return new();
        return await _unitOfWork.ClientRequests.GetByClientIdAsync(clientId);
    }

    public async Task<List<ClientRequest>> GetRequestsByStatusAsync(string status)
    {
        if (string.IsNullOrWhiteSpace(status)) return new();
        return await _unitOfWork.ClientRequests.GetByStatusAsync(status);
    }

    public async Task<ClientRequest?> GetRequestByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return await _unitOfWork.ClientRequests.GetByIdAsync(id);
    }

    public async Task<Dictionary<string, int>> GetStatusCountsAsync(string? clientId = null)
    {
        var requests = string.IsNullOrWhiteSpace(clientId)
            ? await _unitOfWork.ClientRequests.GetAllAsync()
            : await _unitOfWork.ClientRequests.GetByClientIdAsync(clientId);

        return new Dictionary<string, int>
        {
            { "Total",            requests.Count },
            { "Pending",          requests.Count(r => r.Status == "Pending") },
            { "Approved",         requests.Count(r => r.Status == "Approved") },
            { "PartiallyAssigned",requests.Count(r => r.Status == "Partially Assigned") },
            { "Assigned",         requests.Count(r => r.Status == "Assigned") },
            { "Scheduled",        requests.Count(r => r.Status == "Scheduled") },
            { "Active",           requests.Count(r => r.Status == "Active") },
            { "Completed",        requests.Count(r => r.Status == "Completed") },
            { "Rejected",         requests.Count(r => r.Status == "Rejected") },
            { "Cancelled",        requests.Count(r => r.Status == "Cancelled") },
        };
    }

    // ── Client Operations ─────────────────────────────────────────────────────

    public async Task<(bool Success, string Error)> CreateRequestAsync(ClientRequest request)
    {
        if (request == null)
            return (false, "Request payload cannot be empty.");
        if (string.IsNullOrWhiteSpace(request.ClientId))
            return (false, "Client identity is required.");
        if (request.NumberOfGuards <= 0)
            return (false, "You must request at least one security guard.");
        if (string.IsNullOrWhiteSpace(request.Location))
            return (false, "Patrol location is required.");
        if (string.IsNullOrWhiteSpace(request.Duration))
            return (false, "Shift duration is required.");

        request.Status = "Pending";
        request.AssignedGuardIds = new List<string>();

        await _unitOfWork.ClientRequests.AddAsync(request);

        // MODULE 11: Audit trail
        await _auditLog.LogAsync(
            "ClientRequest", request.Id, "Created",
            request.ClientId, toValue: "Pending",
            notes: $"{request.NumberOfGuards} guard(s) at {request.Location}");

        // Notify Admin and Recruiter of new request
        await _notificationService.NotifyRoleAsync(
            "Admin",
            "New Patrol Request Submitted",
            $"A client has submitted a patrol request for {request.NumberOfGuards} guard(s) at {request.Location}.",
            "Info");

        await _notificationService.NotifyRoleAsync(
            "Recruiter",
            "New Guard Deployment Request",
            $"A new deployment request for {request.NumberOfGuards} guard(s) at {request.Location} is pending approval.",
            "Info");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> EditRequestAsync(
        string id, string clientId, string location, string duration, int numberOfGuards, string? description)
    {
        var request = await _unitOfWork.ClientRequests.GetByIdAsync(id);
        if (request == null) return (false, "Request not found.");
        if (request.ClientId != clientId) return (false, "Unauthorized: You can only edit your own requests.");
        if (request.Status != "Pending") return (false, $"Only Pending requests can be edited. Current status: '{request.Status}'.");

        if (string.IsNullOrWhiteSpace(location)) return (false, "Location is required.");
        if (string.IsNullOrWhiteSpace(duration)) return (false, "Duration is required.");
        if (numberOfGuards <= 0) return (false, "At least one guard must be requested.");

        request.Location = location;
        request.Duration = duration;
        request.NumberOfGuards = numberOfGuards;
        request.Description = description;
        _unitOfWork.ClientRequests.Update(request);

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> CancelRequestAsync(string id, string clientId, string? reason = null)
    {
        var request = await _unitOfWork.ClientRequests.GetByIdAsync(id);
        if (request == null) return (false, "Request not found.");
        if (request.ClientId != clientId) return (false, "Unauthorized: You can only cancel your own requests.");

        // Cannot cancel once guards are on duty or deployment is complete
        if (request.Status is "Active" or "Completed" or "Cancelled" or "Rejected")
            return (false, $"A request in '{request.Status}' status cannot be cancelled.");

        await _unitOfWork.ClientRequests.UpdateStatusAsync(id, "Cancelled");

        // MODULE 11: Audit trail
        await _auditLog.LogAsync(
            "ClientRequest", id, "Cancelled",
            clientId, fromValue: request.Status, toValue: "Cancelled",
            notes: reason, performedByRole: "Client");

        // Notify Admin
        await _notificationService.NotifyRoleAsync(
            "Admin",
            "Client Cancelled a Request",
            $"A deployment request for {request.NumberOfGuards} guard(s) at '{request.Location}' was cancelled by the client.",
            "Warning");

        return (true, string.Empty);
    }

    // ── Admin / Recruiter Operations ──────────────────────────────────────────

    public async Task<(bool Success, string Error)> ApproveRequestAsync(string id)
    {
        var request = await _unitOfWork.ClientRequests.GetByIdAsync(id);
        if (request == null) return (false, "Request not found.");
        var (ok, err) = ValidateTransition(request.Status, "Approved");
        if (!ok) return (false, err);

        await _unitOfWork.ClientRequests.UpdateStatusAsync(id, "Approved");

        // MODULE 11: Audit trail
        await _auditLog.LogAsync("ClientRequest", id, "StatusChanged",
            "system", fromValue: request.Status, toValue: "Approved", performedByRole: "Admin");

        await _notificationService.CreateNotificationAsync(
            request.ClientId,
            "Deployment Request Approved",
            $"Your request for {request.NumberOfGuards} guard(s) at '{request.Location}' has been approved. Guards will be assigned shortly.",
            "Info");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> RejectRequestAsync(string id, string? reason = null)
    {
        var request = await _unitOfWork.ClientRequests.GetByIdAsync(id);
        if (request == null) return (false, "Request not found.");
        var (ok, err) = ValidateTransition(request.Status, "Rejected");
        if (!ok) return (false, err);

        await _unitOfWork.ClientRequests.UpdateStatusAsync(id, "Rejected");

        // MODULE 11: Audit trail
        await _auditLog.LogAsync("ClientRequest", id, "StatusChanged",
            "system", fromValue: request.Status, toValue: "Rejected",
            notes: reason, performedByRole: "Admin");

        var msg = string.IsNullOrWhiteSpace(reason)
            ? $"Your deployment request at '{request.Location}' could not be fulfilled at this time. Please contact support."
            : $"Your deployment request at '{request.Location}' was rejected. Reason: {reason}";

        await _notificationService.CreateNotificationAsync(request.ClientId, "Deployment Request Rejected", msg, "Warning");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ScheduleRequestAsync(string id, DateTime scheduledDate)
    {
        var request = await _unitOfWork.ClientRequests.GetByIdAsync(id);
        if (request == null) return (false, "Request not found.");
        var (ok, err) = ValidateTransition(request.Status, "Scheduled");
        if (!ok) return (false, err);

        if (scheduledDate < DateTime.UtcNow)
            return (false, "Scheduled date must be in the future.");

        await _unitOfWork.ClientRequests.UpdateStatusAsync(id, "Scheduled");

        // MODULE 11: Audit trail
        await _auditLog.LogAsync("ClientRequest", id, "StatusChanged",
            "system", fromValue: request.Status, toValue: "Scheduled",
            notes: $"Scheduled for {scheduledDate:yyyy-MM-dd HH:mm} UTC", performedByRole: "Admin");

        await _notificationService.CreateNotificationAsync(
            request.ClientId,
            "Deployment Scheduled",
            $"Your deployment at '{request.Location}' has been officially scheduled for {scheduledDate:yyyy-MM-dd HH:mm} UTC.",
            "Info");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ActivateRequestAsync(string id)
    {
        var request = await _unitOfWork.ClientRequests.GetByIdAsync(id);
        if (request == null) return (false, "Request not found.");
        var (ok, err) = ValidateTransition(request.Status, "Active");
        if (!ok) return (false, err);

        await _unitOfWork.ClientRequests.UpdateStatusAsync(id, "Active");

        // MODULE 11: Audit trail
        await _auditLog.LogAsync("ClientRequest", id, "StatusChanged",
            "system", fromValue: request.Status, toValue: "Active", performedByRole: "Admin");

        // MODULE 1 FIX: Set GuardStatus = OnDuty for each assigned guard
        if (request.AssignedGuardIds != null)
        {
            foreach (var gid in request.AssignedGuardIds)
            {
                await _unitOfWork.Users.UpdateGuardStatusAsync(gid, "OnDuty");
            }
        }

        await _notificationService.CreateNotificationAsync(
            request.ClientId,
            "Deployment Now Active",
            $"Your security deployment at '{request.Location}' is now active. Guards are on duty.",
            "Info");

        return (true, string.Empty);
    }

    // ── Legacy / Internal Operations ──────────────────────────────────────────
    // These are called by GuardApplicationService for guard-count-driven transitions

    public async Task<(bool Success, string Error)> UpdateRequestStatusAsync(string id, string status)
    {
        var existing = await _unitOfWork.ClientRequests.GetByIdAsync(id);
        if (existing == null) return (false, "Client security request not found.");

        var (ok, err) = ValidateTransition(existing.Status, status);
        if (!ok) return (false, err);

        await _unitOfWork.ClientRequests.UpdateStatusAsync(id, status);

        // MODULE 11: Audit trail
        await _auditLog.LogAsync("ClientRequest", id, "StatusChanged",
            "system", fromValue: existing.Status, toValue: status, performedByRole: "Admin");

        // MODULE 1 FIX: Synchronize Guard status based on ClientRequest state
        if (existing.AssignedGuardIds != null && existing.AssignedGuardIds.Any())
        {
            foreach (var gid in existing.AssignedGuardIds)
            {
                if (status == "Completed" || status == "Cancelled" || status == "Rejected")
                {
                    await _unitOfWork.Users.UpdateGuardStatusAsync(gid, "Available");

                    // Update linked AssignedShift records
                    var doneShifts = await _unitOfWork.AssignedShifts.GetByClientRequestIdAsync(id);
                    foreach (var ds in doneShifts.Where(s => s.GuardId == gid && s.Status != "Completed" && s.Status != "Cancelled"))
                        await _unitOfWork.AssignedShifts.UpdateStatusAsync(ds.Id, status == "Completed" ? "Completed" : "Cancelled");

                    await _notificationService.CreateNotificationAsync(
                        gid, "Deployment Concluded",
                        $"Your deployment at '{existing.Location}' has ended. Status: {status}.", "Info");
                }
                else if (status == "Active")
                {
                    await _unitOfWork.Users.UpdateGuardStatusAsync(gid, "OnDuty");

                    // Update linked AssignedShift records to Active
                    var activeShifts = await _unitOfWork.AssignedShifts.GetByClientRequestIdAsync(id);
                    foreach (var aSh in activeShifts.Where(s => s.GuardId == gid && s.Status == "Scheduled"))
                        await _unitOfWork.AssignedShifts.UpdateStatusAsync(aSh.Id, "Active");

                    await _notificationService.CreateNotificationAsync(
                        gid, "Deployment Now Active — Report for Duty",
                        $"Your deployment at '{existing.Location}' is NOW ACTIVE. You are on duty.", "Warning");
                }
                else if (status == "Scheduled")
                {
                    // Auto-create an AssignedShift for the guard linked to this request
                    var existing_shifts = await _unitOfWork.AssignedShifts.GetByClientRequestIdAsync(id);
                    var guardAlreadyHasShift = existing_shifts.Any(s => s.GuardId == gid);

                    if (!guardAlreadyHasShift)
                    {
                        var guardUser = await _unitOfWork.Users.GetByIdAsync(gid);
                        var shiftDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");
                        var newShift = new AssignedShift
                        {
                            GuardId         = gid,
                            GuardName       = guardUser?.Username ?? gid,
                            ShiftDate       = shiftDate,
                            StartTime       = "08:00",
                            EndTime         = "20:00",
                            Status          = "Scheduled",
                            ClientRequestId = id,
                            Location        = existing.Location,
                            Notes           = $"Deployment: {existing.Duration}. Client request #{id.Substring(0, Math.Min(id.Length, 6))}."
                        };
                        await _unitOfWork.AssignedShifts.AddAsync(newShift);
                    }

                    await _notificationService.CreateNotificationAsync(
                        gid, "Deployment Scheduled",
                        $"Your deployment at '{existing.Location}' has been officially scheduled. Please check your shift schedule.", "Info");
                }
                else if (status == "Assigned" || status == "Partially Assigned")
                {
                    await _unitOfWork.Users.UpdateGuardStatusAsync(gid, "Assigned");
                }
            }
        }

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> AssignGuardsToRequestAsync(string id, List<string> guardIds)
    {
        var existing = await _unitOfWork.ClientRequests.GetByIdAsync(id);
        if (existing == null) return (false, "Client security request not found.");

        var (ok, err) = ValidateTransition(existing.Status, "Assigned");
        // Fallback to Partially Assigned check if not all required guards are provided
        if (!ok)
        {
            var (okPart, errPart) = ValidateTransition(existing.Status, "Partially Assigned");
            if (!okPart) return (false, errPart);
        }

        await _unitOfWork.ClientRequests.UpdateAssignedGuardsAsync(id, guardIds);
        
        string targetStatus = (guardIds.Count >= existing.NumberOfGuards) ? "Assigned" : "Partially Assigned";
        await _unitOfWork.ClientRequests.UpdateStatusAsync(id, targetStatus);

        // MODULE 11: Audit trail
        await _auditLog.LogAsync("ClientRequest", id, "StatusChanged",
            "system", fromValue: existing.Status, toValue: targetStatus,
            notes: $"{guardIds.Count} of {existing.NumberOfGuards} guard(s) assigned", performedByRole: "Admin");

        // MODULE 1 FIX: Set GuardStatus = Assigned for all assigned guards
        foreach (var gid in guardIds)
        {
            await _unitOfWork.Users.UpdateGuardStatusAsync(gid, "Assigned");

            await _notificationService.CreateNotificationAsync(
                gid,
                "Assigned to Patrol Detail",
                $"You have been assigned to a patrol detail at '{existing.Location}'. Please report for duty.",
                "Info");
        }

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DeleteRequestAsync(string id)
    {
        var existing = await _unitOfWork.ClientRequests.GetByIdAsync(id);
        if (existing == null) return (false, "Client security request not found.");

        await _unitOfWork.ClientRequests.DeleteAsync(id);
        return (true, string.Empty);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private static (bool, string) ValidateTransition(string currentStatus, string targetStatus)
    {
        if (!_allowedTransitions.TryGetValue(currentStatus, out var allowed))
            return (false, $"Unknown current status: '{currentStatus}'.");

        if (!allowed.Contains(targetStatus))
            return (false, $"Invalid transition: '{currentStatus}' → '{targetStatus}'.");

        return (true, string.Empty);
    }
}
