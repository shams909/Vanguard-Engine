using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IClientRequestService
{
    // ── Queries ──────────────────────────────────────────────────────────────
    Task<List<ClientRequest>> GetAllRequestsAsync();
    Task<List<ClientRequest>> GetRequestsByClientAsync(string clientId);
    Task<List<ClientRequest>> GetRequestsByStatusAsync(string status);
    Task<ClientRequest?> GetRequestByIdAsync(string id);
    Task<Dictionary<string, int>> GetStatusCountsAsync(string? clientId = null);

    // ── Client Operations ─────────────────────────────────────────────────────
    Task<(bool Success, string Error)> CreateRequestAsync(ClientRequest request);

    /// <summary>Client edits a Pending request. Only allowed while Status == Pending.</summary>
    Task<(bool Success, string Error)> EditRequestAsync(string id, string clientId, string location, string duration, int numberOfGuards, string? description);

    /// <summary>Client cancels their own request. Allowed before Assigned state.</summary>
    Task<(bool Success, string Error)> CancelRequestAsync(string id, string clientId, string? reason = null);

    // ── Admin / Recruiter Operations ──────────────────────────────────────────
    /// <summary>Admin approves the request — opens it for guard applications.</summary>
    Task<(bool Success, string Error)> ApproveRequestAsync(string id);

    /// <summary>Admin rejects the request with an optional reason shown to the client.</summary>
    Task<(bool Success, string Error)> RejectRequestAsync(string id, string? reason = null);

    /// <summary>Transition: Assigned → Scheduled with a confirmed start date.</summary>
    Task<(bool Success, string Error)> ScheduleRequestAsync(string id, DateTime scheduledDate);

    /// <summary>Transition: Scheduled → Active. Guards are now officially on duty.</summary>
    Task<(bool Success, string Error)> ActivateRequestAsync(string id);

    // ── Legacy guard-count-driven updates (called by GuardApplicationService) ─
    Task<(bool Success, string Error)> UpdateRequestStatusAsync(string id, string status);
    Task<(bool Success, string Error)> AssignGuardsToRequestAsync(string id, List<string> guardIds);
    Task<(bool Success, string Error)> DeleteRequestAsync(string id);
}

