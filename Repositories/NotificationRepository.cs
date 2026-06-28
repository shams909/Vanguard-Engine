using Appwrite;
using Newtonsoft.Json;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Repositories;

public class NotificationRepository : AppwriteRepository<Notification>, INotificationRepository
{
    public NotificationRepository(IAppwriteService appwriteService)
        : base(appwriteService, "notifications")
    {
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<List<Notification>> GetByUserIdAsync(string userId)
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string>
            {
                Query.Equal("userId", userId),
                Query.OrderDesc("$createdAt"),
                Query.Limit(50)
            }
        );
        return result.Documents.Select(d => MapToEntity(d)!).ToList();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        // Workaround for Appwrite C# SDK bug with boolean query parameters
        var recent = await GetByUserIdAsync(userId);
        return recent.Count(n => !n.IsRead);
    }

    public async Task MarkAsReadAsync(string notificationId)
    {
        await _databases.UpdateDocument(
            databaseId: _databaseId,
            collectionId: _collectionId,
            documentId: notificationId,
            data: new Dictionary<string, object> { { "isRead", true } }
        );
    }

    // ── Override AddAsync with explicit field mapping ──────────────────────
    // The base class uses Pascal-case JSON serialization which doesn't match
    // the Appwrite attribute names (camelCase). We explicitly map fields here.

    public override async Task AddAsync(Notification n)
    {
        await _databases.CreateDocument(
            databaseId: _databaseId,
            collectionId: _collectionId,
            documentId: ID.Unique(),
            data: BuildData(n)
        );
    }

    // ── MapToEntity override to handle camelCase → PascalCase mapping ─────
    protected new Notification? MapToEntity(Appwrite.Models.Document document)
    {
        if (document == null) return null;

        var d = document.Data;

        return new Notification
        {
            Id         = document.Id,
            UserId     = d.TryGetValue("userId",     out var uid)  ? uid?.ToString()  ?? "" : "",
            Title      = d.TryGetValue("title",      out var t)    ? t?.ToString()    ?? "" : "",
            Message    = d.TryGetValue("message",    out var msg)  ? msg?.ToString()  ?? "" : "",
            Type       = d.TryGetValue("type",       out var typ)  ? typ?.ToString()  ?? "Info" : "Info",
            IsRead     = d.TryGetValue("isRead",     out var ir)   && ir is bool b && b,
            CreatedAt  = DateTime.TryParse(document.CreatedAt, out var ca) ? ca : DateTime.UtcNow,
            Expiration = d.TryGetValue("expiration", out var exp) && exp != null
                             ? (DateTime.TryParse(exp.ToString(), out var ed) ? ed : (DateTime?)null)
                             : null
        };
    }

    // ── Private helper ────────────────────────────────────────────────────

    private static Dictionary<string, object> BuildData(Notification n)
    {
        var data = new Dictionary<string, object>
        {
            { "userId",  n.UserId },
            { "title",   n.Title },
            { "message", n.Message },
            { "type",    n.Type },
            { "isRead",  n.IsRead }
        };

        if (n.Expiration.HasValue)
            data["expiration"] = n.Expiration.Value.ToString("o"); // ISO 8601

        return data;
    }

    public async Task MarkAllReadAsync(string userId)
    {
        try
        {
            // Fetch all unread notifications for this user, then patch each one
            var result = await _databases.ListDocuments(
                databaseId:   _databaseId,
                collectionId: _collectionId,
                queries: new List<string>
                {
                    Query.Equal("userId", userId),
                    Query.Equal("isRead", false),
                    Query.Limit(100)
                });

            var tasks = result.Documents.Select(doc =>
                _databases.UpdateDocument(
                    _databaseId, _collectionId, doc.Id,
                    new Dictionary<string, object> { { "isRead", true } }));

            await Task.WhenAll(tasks);
        }
        catch { /* Swallow — non-critical */ }
    }

    public async Task DeleteExpiredAsync(string userId)
    {
        try
        {
            var all    = await GetByUserIdAsync(userId);
            var now    = DateTime.UtcNow;
            var expired = all.Where(n => n.Expiration.HasValue && n.Expiration.Value < now).ToList();

            var tasks = expired.Select(n =>
                _databases.DeleteDocument(_databaseId, _collectionId, n.Id));

            await Task.WhenAll(tasks);
        }
        catch { /* Swallow — non-critical */ }
    }

    public async Task DeleteAsync(string id)
    {
        try { await _databases.DeleteDocument(_databaseId, _collectionId, id); }
        catch { }
    }

    public async Task DeleteManyAsync(List<string> ids)
    {
        try 
        {
            var tasks = ids.Select(id => _databases.DeleteDocument(_databaseId, _collectionId, id));
            await Task.WhenAll(tasks);
        }
        catch { }
    }
}
