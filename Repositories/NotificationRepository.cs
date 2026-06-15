using Appwrite;
using Vanguard_Engine.Entities;
using Vanguard_Engine.Services;
using Vanguard_Engine.Repositories;

namespace Vanguard_Engine.Repositories;

public class NotificationRepository : AppwriteRepository<Notification>, INotificationRepository
{
    public NotificationRepository(IAppwriteService appwriteService)
        : base(appwriteService, "notifications")
    {
    }

    public async Task<List<Notification>> GetByUserIdAsync(string userId)
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string> { Query.Equal("userId", userId), Query.OrderDesc("$createdAt") }
        );
        return result.Documents.Select(d => MapToEntity(d)!).ToList();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        var result = await _databases.ListDocuments(
            databaseId: _databaseId,
            collectionId: _collectionId,
            queries: new List<string> { Query.Equal("userId", userId), Query.Equal("isRead", false) }
        );
        return result.Documents.Count;
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
}
