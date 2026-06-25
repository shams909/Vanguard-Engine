using Appwrite;
using Appwrite.Services;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Repositories;

public class AppwriteRatingRepository : AppwriteRepository<Rating>, IRatingRepository
{
    public AppwriteRatingRepository(IAppwriteService appwriteService)
        : base(appwriteService, "ratings")
    {
    }

    public async Task<List<Rating>> GetByGuardIdAsync(string guardId)
    {
        try
        {
            var result = await _databases.ListDocuments(
                databaseId: _databaseId,
                collectionId: _collectionId,
                queries: new List<string> { Query.Equal("guardId", guardId) }
            );
            return result.Documents.Select(d => MapToEntity(d)!).ToList();
        }
        catch { return new List<Rating>(); }
    }

    public async Task<List<Rating>> GetByClientIdAsync(string clientId)
    {
        try
        {
            var result = await _databases.ListDocuments(
                databaseId: _databaseId,
                collectionId: _collectionId,
                queries: new List<string> { Query.Equal("clientId", clientId) }
            );
            return result.Documents.Select(d => MapToEntity(d)!).ToList();
        }
        catch { return new List<Rating>(); }
    }
}
