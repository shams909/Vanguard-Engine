using Appwrite;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Repositories;

public class HiringNoticeRepository : AppwriteRepository<HiringNotice>, IHiringNoticeRepository
{
    public HiringNoticeRepository(IAppwriteService appwriteService)
        : base(appwriteService, "hiring_notices")
    {
    }

    public async Task<List<HiringNotice>> GetAllAsync()
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string> { Query.OrderDesc("$createdAt"), Query.Limit(100) }
        );
        return result.Documents.Select(d => MapToEntity(d)!).ToList();
    }

    public async Task<List<HiringNotice>> GetOpenNoticesAsync()
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string>
            {
                Query.Equal("status", "Open"),
                Query.OrderDesc("$createdAt")
            }
        );
        return result.Documents.Select(d => MapToEntity(d)!).ToList();
    }

    public override async Task<HiringNotice?> GetByIdAsync(string id)
    {
        try {
            var doc = await _databases.GetDocument(_databaseId, _collectionId, id);
            return MapToEntity(doc);
        } catch { return null; }
    }

    public async Task UpdateAsync(HiringNotice notice)
    {
        await _databases.UpdateDocument(
            databaseId: _databaseId,
            collectionId: _collectionId,
            documentId: notice.Id,
            data: BuildData(notice)
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

    public async Task UpdateFilledPositionsAsync(string id, int filledCount)
    {
        await _databases.UpdateDocument(
            databaseId: _databaseId,
            collectionId: _collectionId,
            documentId: id,
            data: new Dictionary<string, object> { { "filledPositions", filledCount } }
        );
    }

    public async Task DeleteAsync(string id)
    {
        await _databases.DeleteDocument(_databaseId, _collectionId, id);
    }

    public override async Task AddAsync(HiringNotice entity)
    {
        await _databases.CreateDocument(_databaseId, _collectionId, ID.Unique(), BuildData(entity));
    }

    private static Dictionary<string, object> BuildData(HiringNotice e) => new()
    {
        { "title",             e.Title },
        { "referenceCode",     e.ReferenceCode },
        { "description",       e.Description },
        { "requirements",      e.Requirements },
        { "location",          e.Location },
        { "jobType",           e.JobType },
        { "priority",          e.Priority },
        { "salaryRange",       e.SalaryRange ?? string.Empty },
        { "status",            e.Status },
        { "postedByUserId",    e.PostedByUserId },
        { "expiryDate",        e.ExpiryDate?.ToString("o") ?? string.Empty },
        { "numberOfPositions", e.NumberOfPositions },
        { "filledPositions",   e.FilledPositions },
    };
}

