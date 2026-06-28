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

    /// <summary>Client cancels their own pending/approved request. Admin can cancel up to Active.</summary>
    Task<(bool Success, string Error)> CancelRequestAsync(string id, string requesterId, bool isAdmin = false);

    /// <summary>Hard-delete — Admin only. Use CancelRequestAsync for normal lifecycle cancellation.</summary>
    Task<(bool Success, string Error)> DeleteRequestAsync(string id);

    // ── Admin Workflow ────────────────────────────────────────────────────
    Task<(bool Success, string Error)> UpdateRequestStatusAsync(string id, string status);
    Task<(bool Success, string Error)> ApproveRequestAsync(string id);
    Task<(bool Success, string Error)> RejectRequestAsync(string id);
    Task<(bool Success, string Error)> CompleteRequestAsync(string id);

    // ── Guard Assignment ──────────────────────────────────────────────────
    /// <param name="armedRequired">When true, only returns guards with ArmedLicense = true.</param>
    Task<List<GuardApplication>> GetEligibleGuardsAsync(bool armedRequired = false);
    Task<(bool Success, string Error)> AssignGuardsAsync(string requestId, List<string> guardUserIds);
    Task<(bool Success, string Error)> ScheduleProtectionAsync(string id);
    Task<(bool Success, string Error)> StartProtectionAsync(string id);

    // ── Dashboard Stats ───────────────────────────────────────────────────
    Task<Dictionary<string, int>> GetStatusCountsAsync(string clientId);
    Task<Dictionary<string, int>> GetAllStatusCountsAsync();
}

