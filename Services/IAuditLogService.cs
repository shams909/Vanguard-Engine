using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IAuditLogService
{
    /// <summary>
    /// MODULE 11: Records an immutable audit event for any state transition.
    /// Call this after every successful status change in a service.
    /// </summary>
    Task LogAsync(
        string entityType,
        string entityId,
        string action,
        string performedByUserId,
        string? fromValue         = null,
        string? toValue           = null,
        string? notes             = null,
        string? performedByRole   = null);

    Task<List<AuditLog>> GetEntityHistoryAsync(string entityType, string entityId);
    Task<List<AuditLog>> GetUserActivityAsync(string userId);
    Task<List<AuditLog>> GetRecentActivityAsync(int limit = 50);
}
