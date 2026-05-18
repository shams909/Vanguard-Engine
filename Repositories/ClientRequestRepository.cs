using Appwrite;
using Newtonsoft.Json;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Repositories;

public class ClientRequestRepository : AppwriteRepository<ClientRequest>, IClientRequestRepository
{
    public ClientRequestRepository(IAppwriteService appwriteService)
        : base(appwriteService, "client_requests")
    {
    }

    public async Task<List<ClientRequest>> GetAllAsync()
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string> { Query.OrderDesc("$createdAt"), Query.Limit(100) }
        );
        return result.Documents.Select(d => MapToEntity(d)!).ToList();
    }

    public async Task<List<ClientRequest>> GetByClientIdAsync(string clientId)
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string>
            {
                Query.Equal("clientId", clientId),
                Query.OrderDesc("$createdAt"),
                Query.Limit(100)
            }
        );
        return result.Documents.Select(d => MapToEntity(d)!).ToList();
    }

    public async Task<List<ClientRequest>> GetByStatusAsync(string status)
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

    public async Task UpdateAsync(ClientRequest request)
    {
        await _databases.UpdateDocument(
            databaseId: _databaseId,
            collectionId: _collectionId,
            documentId: request.Id,
            data: BuildData(request)
        );
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

    public override async Task AddAsync(ClientRequest entity)
    {
        await _databases.CreateDocument(_databaseId, _collectionId, ID.Unique(), BuildData(entity));
    }

    private static Dictionary<string, object> BuildData(ClientRequest r) => new()
    {
        { "clientId", r.ClientId },
        { "numberOfGuards", r.NumberOfGuards },
        { "location", r.Location },
        { "duration", r.Duration },
        { "status", r.Status },
        { "assignedGuardIds", r.AssignedGuardIds }
    };
}
