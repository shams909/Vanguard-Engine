using Appwrite;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Repositories;

public class VIPRequestRepository : AppwriteRepository<VIPRequest>, IVIPRequestRepository
{
    public VIPRequestRepository(IAppwriteService appwriteService)
        : base(appwriteService, "vip_requests")
    {
    }

    public async Task<List<VIPRequest>> GetAllAsync()
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string> { Query.OrderDesc("$createdAt"), Query.Limit(100) }
        );
        return result.Documents.Select(d => MapToEntity(d)!).ToList();
    }

    public async Task<List<VIPRequest>> GetByClientIdAsync(string clientId)
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string>
            {
                Query.Equal("vipClientId", clientId),
                Query.OrderDesc("$createdAt"),
                Query.Limit(100)
            }
        );
        return result.Documents.Select(d => MapToEntity(d)!).ToList();
    }

    public async Task<List<VIPRequest>> GetByStatusAsync(string status)
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string>
            {
                Query.Equal("status", status),
                Query.OrderDesc("$createdAt"),
                Query.Limit(100)
            }
        );
        return result.Documents.Select(d => MapToEntity(d)!).ToList();
    }

    public async Task UpdateStatusAsync(string id, string status)
    {
        await _databases.UpdateDocument(
            databaseId: _databaseId,
            collectionId: _collectionId,
            documentId: id,
            data: new Dictionary<string, object> { { "status", status } }
        );
    }

    public async Task UpdateAssignedGuardsAsync(string id, List<string> guardIds)
    {
        await _databases.UpdateDocument(
            databaseId: _databaseId,
            collectionId: _collectionId,
            documentId: id,
            data: new Dictionary<string, object> { { "assignedGuardIds", guardIds } }
        );
    }

    public async Task DeleteAsync(string id)
    {
        await _databases.DeleteDocument(_databaseId, _collectionId, id);
    }

    public override async Task AddAsync(VIPRequest entity)
    {
        await _databases.CreateDocument(_databaseId, _collectionId, ID.Unique(), BuildData(entity));
    }

    private static Dictionary<string, object> BuildData(VIPRequest r) => new()
    {
        { "vipClientId",       r.VipClientId },
        { "protectionType",    r.ProtectionType },
        { "armedRequired",     r.ArmedRequired },
        { "numberOfGuards",    r.NumberOfGuards },
        { "duration",          r.Duration },
        { "status",            r.Status },
        { "assignedGuardIds",  r.AssignedGuardIds }
    };
}
