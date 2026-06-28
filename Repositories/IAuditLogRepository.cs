using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Repositories;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    Task<List<AuditLog>> GetByEntityAsync(string entityType, string entityId);
    Task<List<AuditLog>> GetByUserAsync(string userId);
    Task<List<AuditLog>> GetRecentAsync(int limit = 50);
}
