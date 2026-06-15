using Appwrite;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Repositories;

public class GuardShiftRepository : AppwriteRepository<GuardShift>, IGuardShiftRepository
{
    public GuardShiftRepository(IAppwriteService appwriteService)
        : base(appwriteService, "guard_shifts") { }

    public async Task<List<GuardShift>> GetAllAsync()
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string> { Query.OrderDesc("$createdAt"), Query.Limit(200) }
        );
        return result.Documents.Select(d => MapToEntity(d)!).ToList();
    }

    public async Task<List<GuardShift>> GetByGuardIdAsync(string guardId)
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string>
            {
                Query.Equal("guardId", guardId),
                Query.OrderDesc("$createdAt"),
                Query.Limit(100)
            }
        );
        return result.Documents.Select(d => MapToEntity(d)!).ToList();
    }

    public async Task<GuardShift?> GetActiveShiftAsync(string guardId)
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string>
            {
                Query.Equal("guardId", guardId),
                Query.Equal("status", "Active"),
                Query.Limit(1)
            }
        );
        return result.Documents.Select(d => MapToEntity(d)!).FirstOrDefault();
    }

    public async Task<List<GuardShift>> GetByStatusAsync(string status)
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

    public async Task UpdateCheckOutAsync(string id, string checkOutTime, int durationMinutes)
    {
        await _databases.UpdateDocument(
            databaseId: _databaseId,
            collectionId: _collectionId,
            documentId: id,
            data: new Dictionary<string, object>
            {
                { "checkOutTime",    checkOutTime },
                { "durationMinutes", durationMinutes },
                { "status",          "Completed" }
            }
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

    public async Task DeleteAsync(string id) =>
        await _databases.DeleteDocument(_databaseId, _collectionId, id);

    public override async Task AddAsync(GuardShift entity)
    {
        await _databases.CreateDocument(
            databaseId: _databaseId,
            collectionId: _collectionId,
            documentId: ID.Unique(),
            data: BuildData(entity)
        );
    }

    private static Dictionary<string, object> BuildData(GuardShift s) => new()
    {
        { "guardId",         s.GuardId },
        { "guardName",       s.GuardName },
        { "checkInTime",     s.CheckInTime },
        // Send empty string for optional fields on creation — entity parser handles "" as null
        { "checkOutTime",    s.CheckOutTime ?? string.Empty },
        { "durationMinutes", s.DurationMinutes },
        { "status",          s.Status }
    };
}
