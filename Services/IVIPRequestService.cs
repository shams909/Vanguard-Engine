using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IVIPRequestService
{
    // ── Queries ──────────────────────────────────────────────────────────────
    Task<List<VIPRequest>> GetAllRequestsAsync();
    Task<List<VIPRequest>> GetRequestsByClientAsync(string clientId);
    Task<List<VIPRequest>> GetRequestsByStatusAsync(string status);
    Task<VIPRequest?> GetRequestByIdAsync(string id);

    // ── VIP Client Operations ─────────────────────────────────────────────
    Task<(bool Success, string Error)> CreateRequestAsync(VIPRequest request);
    Task<(bool Success, string Error)> DeleteRequestAsync(string id);

    // ── Admin Workflow ────────────────────────────────────────────────────
    Task<(bool Success, string Error)> UpdateRequestStatusAsync(string id, string status);
    Task<(bool Success, string Error)> ApproveRequestAsync(string id);
    Task<(bool Success, string Error)> RejectRequestAsync(string id);
    Task<(bool Success, string Error)> CompleteRequestAsync(string id);

    // ── Phase 3: Elite Guard Assignment ──────────────────────────────────
    Task<List<GuardApplication>> GetEligibleGuardsAsync();
    Task<(bool Success, string Error)> AssignGuardsAsync(string requestId, List<string> guardUserIds);
    Task<(bool Success, string Error)> StartProtectionAsync(string id);

    // ── Dashboard Stats ───────────────────────────────────────────────────
    Task<Dictionary<string, int>> GetStatusCountsAsync(string clientId);
    Task<Dictionary<string, int>> GetAllStatusCountsAsync();
}
