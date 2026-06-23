using Appwrite;
using Appwrite.Services;
using Newtonsoft.Json;
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
                queries: new List<string> { Query.Equal("guardId", guardId) }
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
                queries: new List<string> { 
                    Query.GreaterThanEqual("shiftDate", start.ToString("yyyy-MM-dd")),
                    Query.LessThanEqual("shiftDate", end.ToString("yyyy-MM-dd"))
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
                collectionId: _collectionId
            );
            return result.Documents.Select(d => MapToEntity(d)!).ToList();
        }
        catch { return new List<AssignedShift>(); }
    }
}
