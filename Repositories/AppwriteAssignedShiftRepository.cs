using Appwrite;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Repositories;

public class AppwriteAssignedShiftRepository : AppwriteRepository<AssignedShift>, IAssignedShiftRepository
{
    public AppwriteAssignedShiftRepository(IAppwriteService appwriteService)
        : base(appwriteService, "assigned_shifts")
    {
    }

    public async Task<List<AssignedShift>> GetByGuardIdAsync(string guardId)
    {
        try
        {
            var result = await _databases.ListDocuments(
                databaseId: _databaseId,
                collectionId: _collectionId,
                queries: new List<string> { Query.Equal("guardId", guardId), Query.OrderDesc("$createdAt") }
            );
            return result.Documents.Select(d => MapToEntity(d)!).ToList();
        }
        catch { return new List<AssignedShift>(); }
    }

    public async Task<List<AssignedShift>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        try
        {
            var result = await _databases.ListDocuments(
                databaseId: _databaseId,
                collectionId: _collectionId,
                queries: new List<string>
                {
                    Query.GreaterThanEqual("shiftDate", start.ToString("yyyy-MM-dd")),
                    Query.LessThanEqual("shiftDate",   end.ToString("yyyy-MM-dd"))
                }
            );
            return result.Documents.Select(d => MapToEntity(d)!).ToList();
        }
        catch { return new List<AssignedShift>(); }
    }

    public async Task<List<AssignedShift>> GetByStatusAsync(string status)
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
        catch { return new List<AssignedShift>(); }
    }

    public async Task<List<AssignedShift>> GetAllAssignedShiftsAsync()
    {
        try
        {
            var result = await _databases.ListDocuments(
                databaseId: _databaseId,
                collectionId: _collectionId,
                queries: new List<string> { Query.OrderDesc("$createdAt"), Query.Limit(200) }
            );
            return result.Documents.Select(d => MapToEntity(d)!).ToList();
        }
        catch { return new List<AssignedShift>(); }
    }

    public async Task<List<AssignedShift>> GetByClientRequestIdAsync(string clientRequestId)
    {
        try
        {
            var result = await _databases.ListDocuments(
                databaseId: _databaseId,
                collectionId: _collectionId,
                queries: new List<string> { Query.Equal("clientRequestId", clientRequestId) }
            );
            return result.Documents.Select(d => MapToEntity(d)!).ToList();
        }
        catch { return new List<AssignedShift>(); }
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

    public override void Update(AssignedShift shift)
    {
        // Fire-and-forget synchronous wrapper — used from legacy code.
        // Prefer UpdateStatusAsync for new code paths.
        _ = _databases.UpdateDocument(
            databaseId: _databaseId,
            collectionId: _collectionId,
            documentId: shift.Id,
            data: BuildData(shift)
        );
    }

    public override async Task AddAsync(AssignedShift entity)
    {
        await _databases.CreateDocument(
            _databaseId, _collectionId, ID.Unique(), BuildData(entity));
    }

    private static Dictionary<string, object> BuildData(AssignedShift s) => new()
    {
        { "guardId",         s.GuardId },
        { "guardName",       s.GuardName },
        { "shiftDate",       s.ShiftDate },
        { "startTime",       s.StartTime },
        { "endTime",         s.EndTime },
        { "status",          s.Status },
        { "clientRequestId", s.ClientRequestId ?? string.Empty },
        { "location",        s.Location         ?? string.Empty },
        { "notes",           s.Notes            ?? string.Empty },
    };
}

