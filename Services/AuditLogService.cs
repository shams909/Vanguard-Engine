using Microsoft.Extensions.Logging;
using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(IUnitOfWork unitOfWork, ILogger<AuditLogService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger     = logger;
    }

    /// <summary>
    /// MODULE 11: Fire-and-never-throw audit recorder.
    /// Called after every successful state transition — NEVER blocks or throws.
    /// </summary>
    public async Task LogAsync(
        string entityType,
        string entityId,
        string action,
        string performedByUserId,
        string? fromValue       = null,
        string? toValue         = null,
        string? notes           = null,
        string? performedByRole = null)
    {
        try
        {
            var entry = new AuditLog
            {
                EntityType        = entityType,
                EntityId          = entityId,
                Action            = action,
                FromValue         = fromValue,
                ToValue           = toValue,
                Notes             = notes,
                PerformedByUserId = performedByUserId,
                PerformedByRole   = performedByRole,
                CreatedAt         = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(entry);
        }
        catch (Exception ex)
        {
            // Audit logging must NEVER break the primary operation
            _logger.LogWarning(ex,
                "[Audit] Failed to write audit log for {EntityType}/{EntityId} action={Action}.",
                entityType, entityId, action);
        }
    }

    public async Task<List<AuditLog>> GetEntityHistoryAsync(string entityType, string entityId)
    {
        try { return await _unitOfWork.AuditLogs.GetByEntityAsync(entityType, entityId); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Audit] GetEntityHistoryAsync failed for {EntityType}/{EntityId}.", entityType, entityId);
            return new List<AuditLog>();
        }
    }

    public async Task<List<AuditLog>> GetUserActivityAsync(string userId)
    {
        try { return await _unitOfWork.AuditLogs.GetByUserAsync(userId); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Audit] GetUserActivityAsync failed for user {UserId}.", userId);
            return new List<AuditLog>();
        }
    }

    public async Task<List<AuditLog>> GetRecentActivityAsync(int limit = 50)
    {
        try { return await _unitOfWork.AuditLogs.GetRecentAsync(limit); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Audit] GetRecentActivityAsync failed.");
            return new List<AuditLog>();
        }
    }
}
