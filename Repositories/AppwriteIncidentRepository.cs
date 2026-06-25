using Appwrite;
using Appwrite.Services;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Repositories;

public class AppwriteIncidentRepository : AppwriteRepository<Incident>, IIncidentRepository
{
    public AppwriteIncidentRepository(IAppwriteService appwriteService)
        : base(appwriteService, "incidents")
    {
    }

    public async Task<List<Incident>> GetByReporterAsync(string userId)
    {
        try
        {
            var result = await _databases.ListDocuments(
                databaseId: _databaseId,
                collectionId: _collectionId,
                queries: new List<string> { Query.Equal("reportedByUserId", userId) }
            );
            return result.Documents.Select(d => MapToEntity(d)!).ToList();
        }
        catch { return new List<Incident>(); }
    }

    public async Task<List<Incident>> GetByStatusAsync(string status)
    {
        try
        {
            var result = await _databases.ListDocuments(
                databaseId: _databaseId,
                collectionId: _collectionId,
                queries: new List<string> { Query.Equal("status", status) }
            );
            return result.Documents.Select(d => MapToEntity(d)!).ToList();
        }
        catch { return new List<Incident>(); }
    }

    public async Task<List<Incident>> GetAllIncidentsAsync()
    {
        try
        {
            var result = await _databases.ListDocuments(
                databaseId: _databaseId,
                collectionId: _collectionId
            );
            return result.Documents.Select(d => MapToEntity(d)!).ToList();
        }
        catch { return new List<Incident>(); }
    }
}
