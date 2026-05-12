using Appwrite;
using Newtonsoft.Json;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Repositories;

public class GuardApplicationRepository : AppwriteRepository<GuardApplication>, IGuardApplicationRepository
{
    public GuardApplicationRepository(IAppwriteService appwriteService)
        : base(appwriteService, "guard_applications")
    {
    }

    public async Task<GuardApplication?> GetByUserIdAsync(string userId)
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string> { Query.Equal("userId", userId) }
        );

        return result.Documents.Count > 0 ? MapToEntity(result.Documents[0]) : null;
    }

    public async Task<List<GuardApplication>> GetAllAsync()
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string> { Query.OrderDesc("$createdAt"), Query.Limit(100) }
        );

        return result.Documents.Select(d => MapToEntity(d)!).ToList();
    }

    public async Task UpdateAsync(GuardApplication application)
    {
        var data = new Dictionary<string, object>
        {
            { "userId", application.UserId },
            { "experience", application.Experience },
            { "skills", application.Skills },
            { "status", application.Status }
        };

        await _databases.UpdateDocument(_databaseId, _collectionId, application.Id, data);
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

    public async Task DeleteAsync(string id)
    {
        await _databases.DeleteDocument(_databaseId, _collectionId, id);
    }

    // Override AddAsync to ensure we only send lowercase Appwrite-mapped fields
    public override async Task AddAsync(GuardApplication entity)
    {
        var data = new Dictionary<string, object>
        {
            { "userId", entity.UserId },
            { "experience", entity.Experience },
            { "skills", entity.Skills },
            { "status", entity.Status }
        };

        await _databases.CreateDocument(_databaseId, _collectionId, ID.Unique(), data);
    }
}
