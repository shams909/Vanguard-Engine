using Appwrite;
using Appwrite.Services;
using Appwrite.Models;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Repositories;

public abstract class AppwriteRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly Databases _databases;
    protected readonly string _databaseId;
    protected readonly string _collectionId;

    public AppwriteRepository(IAppwriteService appwriteService, string collectionId)
    {
        _databases = new Databases(appwriteService.GetClient());
        _databaseId = appwriteService.DatabaseId;
        _collectionId = collectionId;
    }

    protected T? MapToEntity(Document document)
    {
        if (document == null) return null;
        
        var data = document.Data;
        if (!data.ContainsKey("$id"))
        {
            data["$id"] = document.Id;
        }

        var json = JsonConvert.SerializeObject(data);
        return JsonConvert.DeserializeObject<T>(json);
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        try
        {
            var document = await _databases.GetDocument(_databaseId, _collectionId, id);
            return MapToEntity(document);
        }
        catch
        {
            return null;
        }
    }

    public virtual async Task<List<T>> GetPagedAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var result = await _databases.ListDocuments(_databaseId, _collectionId, 
            queries: new List<string> { Query.Limit(pageSize), Query.Offset((pageNumber - 1) * pageSize) });
        
        return result.Documents.Select(d => MapToEntity(d)!).ToList();
    }

    public virtual async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        // Simple implementation - in reality, we'd need to convert Expression to Appwrite Query
        // For now, we'll fetch all and filter locally, but this is NOT efficient for production.
        // A better way is to implement specific query methods.
        var result = await _databases.ListDocuments(_databaseId, _collectionId);
        var items = result.Documents.Select(d => MapToEntity(d)!).ToList();
        return items.AsQueryable().Where(predicate).ToList();
    }

    public virtual async Task AddAsync(T entity)
    {
        var json = JsonConvert.SerializeObject(entity);
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        
        string documentId = ID.Unique();
        
        // Check for existing ID to allow synchronization between Auth and Database
        if (data != null && data.ContainsKey("$id") && !string.IsNullOrEmpty(data["$id"]?.ToString()))
        {
            documentId = data["$id"].ToString()!;
        }
        else if (data != null && data.ContainsKey("Id") && !string.IsNullOrEmpty(data["Id"]?.ToString()))
        {
            documentId = data["Id"].ToString()!;
        }

        // Remove Appwrite metadata fields and C# Id field if they exist
        string[] metadataFields = { "$id", "$createdAt", "$updatedAt", "$permissions", "$databaseId", "$collectionId", "Id", "id" };
        foreach (var field in metadataFields)
        {
            data?.Remove(field);
        }

        await _databases.CreateDocument(_databaseId, _collectionId, documentId, data!);
    }

    public virtual void Update(T entity)
    {
        var json = JsonConvert.SerializeObject(entity);
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        
        if (data == null || !data.ContainsKey("$id")) return;

        var id = data["$id"].ToString();
        
        // Remove Appwrite metadata fields from data
        string[] metadataFields = { "$id", "$createdAt", "$updatedAt", "$permissions", "$databaseId", "$collectionId", "Id", "id" };
        foreach (var field in metadataFields)
        {
            data.Remove(field);
        }
        
        _databases.UpdateDocument(_databaseId, _collectionId, id!, data).Wait();
    }

    public virtual void Remove(T entity)
    {
        var json = JsonConvert.SerializeObject(entity);
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        var id = data!["$id"].ToString();
        
        _databases.DeleteDocument(_databaseId, _collectionId, id!).Wait();
    }
}
