using Appwrite;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Repositories;

public class AppwriteAuditLogRepository : AppwriteRepository<AuditLog>, IAuditLogRepository
{
    public AppwriteAuditLogRepository(IAppwriteService appwriteService)
        : base(appwriteService, "audit_logs")
    {
    }

    public async Task<List<AuditLog>> GetByEntityAsync(string entityType, string entityId)
    {
        try
        {
            var result = await _databases.ListDocuments(
                databaseId:   _databaseId,
                collectionId: _collectionId,
                queries: new List<string>
                {
                    Query.Equal("entityType", entityType),
                    Query.Equal("entityId",   entityId),
                    Query.OrderDesc("$createdAt"),
                    Query.Limit(100)
                });
            return result.Documents.Select(d => MapToEntity(d)!).ToList();
        }
        catch { return new List<AuditLog>(); }
    }

    public async Task<List<AuditLog>> GetByUserAsync(string userId)
    {
        try
        {
            var result = await _databases.ListDocuments(
                databaseId:   _databaseId,
                collectionId: _collectionId,
                queries: new List<string>
                {
                    Query.Equal("performedByUserId", userId),
                    Query.OrderDesc("$createdAt"),
                    Query.Limit(100)
                });
            return result.Documents.Select(d => MapToEntity(d)!).ToList();
        }
        catch { return new List<AuditLog>(); }
    }

    public async Task<List<AuditLog>> GetRecentAsync(int limit = 50)
    {
        try
        {
            var result = await _databases.ListDocuments(
                databaseId:   _databaseId,
                collectionId: _collectionId,
                queries: new List<string>
                {
                    Query.OrderDesc("$createdAt"),
                    Query.Limit(limit)
                });
            return result.Documents.Select(d => MapToEntity(d)!).ToList();
        }
        catch { return new List<AuditLog>(); }
    }

    public override async Task AddAsync(AuditLog entity)
    {
        await _databases.CreateDocument(
            _databaseId, _collectionId, ID.Unique(), new Dictionary<string, object>
            {
                { "entityType",        entity.EntityType },
                { "entityId",          entity.EntityId },
                { "action",            entity.Action },
                { "fromValue",         entity.FromValue   ?? string.Empty },
                { "toValue",           entity.ToValue     ?? string.Empty },
                { "notes",             entity.Notes       ?? string.Empty },
                { "performedByUserId", entity.PerformedByUserId },
                { "performedByRole",   entity.PerformedByRole  ?? string.Empty },
            });
    }
}
