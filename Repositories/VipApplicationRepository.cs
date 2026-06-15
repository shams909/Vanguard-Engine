using Appwrite;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Repositories;

public class VipApplicationRepository : AppwriteRepository<VipApplication>, IVipApplicationRepository
{
    public VipApplicationRepository(IAppwriteService appwriteService)
        : base(appwriteService, "vip_applications") { }

    public async Task<List<VipApplication>> GetAllAsync()
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string> { Query.OrderDesc("$createdAt"), Query.Limit(200) }
        );
        return result.Documents.Select(d => MapToEntity(d)!).ToList();
    }

    public async Task<List<VipApplication>> GetByClientIdAsync(string clientId)
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

    public async Task<VipApplication?> GetPendingApplicationAsync(string clientId)
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string>
            {
                Query.Equal("clientId", clientId),
                Query.Equal("status", "Pending"),
                Query.Limit(1)
            }
        );
        return result.Documents.Select(d => MapToEntity(d)!).FirstOrDefault();
    }

    public async Task<List<VipApplication>> GetByStatusAsync(string status)
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string>
            {
                Query.Equal("status", status),
                Query.OrderDesc("$createdAt"),
                Query.Limit(200)
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

    public override async Task AddAsync(VipApplication entity)
    {
        await _databases.CreateDocument(
            databaseId: _databaseId,
            collectionId: _collectionId,
            documentId: ID.Unique(),
            data: new Dictionary<string, object>
            {
                { "clientId", entity.ClientId },
                { "clientName", entity.ClientName },
                { "companyName", entity.CompanyName },
                { "verificationDetails", entity.VerificationDetails },
                { "status", entity.Status }
            }
        );
    }
}
