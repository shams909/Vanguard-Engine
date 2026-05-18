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
            queries: new List<string>
            {
                Query.Equal("userId", userId),
                Query.OrderDesc("$createdAt"),
                Query.Limit(1)
            }
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
        await _databases.UpdateDocument(_databaseId, _collectionId, application.Id,
            BuildData(application));
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

    public async Task UpdateGuardStatusAsync(string id, string status)
    {
        await _databases.UpdateDocument(
            databaseId: _databaseId,
            collectionId: _collectionId,
            documentId: id,
            data: new Dictionary<string, object> { { "guardStatus", status } }
        );
    }

    public async Task<List<GuardApplication>> GetByJobIdAsync(string jobId)
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string>
            {
                Query.Equal("jobId", jobId),
                Query.OrderDesc("$createdAt"),
                Query.Limit(100)
            }
        );
        return result.Documents.Select(d => MapToEntity(d)!).ToList();
    }

    public async Task DeleteAsync(string id)
    {
        await _databases.DeleteDocument(_databaseId, _collectionId, id);
    }

    public override async Task AddAsync(GuardApplication entity)
    {
        await _databases.CreateDocument(_databaseId, _collectionId, ID.Unique(), BuildData(entity));
    }

    private static Dictionary<string, object> BuildData(GuardApplication e) => new()
    {
        { "userId", e.UserId },
        { "fullName", e.FullName },
        { "phone", e.Phone },
        { "nationalId", e.NationalId },
        { "address", e.Address },
        { "yearsOfExperience", e.YearsOfExperience },
        { "experience", e.Experience },
        { "skills", e.Skills },
        { "preferredLocation", e.PreferredLocation },
        { "armedLicense", e.ArmedLicense },
        { "status", e.Status },
        { "jobId", e.JobId ?? "" },
        { "guardStatus", e.GuardStatus }
    };
}
