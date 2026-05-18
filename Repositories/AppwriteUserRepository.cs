using Appwrite;
using Appwrite.Services;
using User = Vanguard_Engine.Entities.User;
using Vanguard_Engine.Services;
using Newtonsoft.Json;

namespace Vanguard_Engine.Repositories;

public class AppwriteUserRepository : AppwriteRepository<User>, IUserRepository
{
    public AppwriteUserRepository(IAppwriteService appwriteService) : base(appwriteService, "users")
    {
    }

    public override async Task<User?> GetByIdAsync(string id)
    {
        var user = await base.GetByIdAsync(id);
        if (user != null && !string.IsNullOrEmpty(user.RoleId))
        {
            user.Role = await GetRoleAsync(user.RoleId);
        }
        return user;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var result = await _databases.ListDocuments(_databaseId, _collectionId, 
            queries: new List<string> { Query.Equal("email", email) });
        
        if (result.Total == 0) return null;
        
        var user = MapToEntity(result.Documents[0]);
        if (user != null && !string.IsNullOrEmpty(user.RoleId))
        {
            user.Role = await GetRoleAsync(user.RoleId);
        }
        return user;
    }

    public async Task<User?> GetByResetTokenAsync(string token)
    {
        var result = await _databases.ListDocuments(_databaseId, _collectionId, 
            queries: new List<string> { Query.Equal("resetToken", token) });
        
        if (result.Total == 0) return null;
        
        var user = MapToEntity(result.Documents[0]);
        if (user != null && !string.IsNullOrEmpty(user.RoleId))
        {
            user.Role = await GetRoleAsync(user.RoleId);
        }
        return user;
    }

    private async Task<Vanguard_Engine.Entities.Role?> GetRoleAsync(string roleId)
    {
        try
        {
            var doc = await _databases.GetDocument(_databaseId, "roles", roleId);
            var json = JsonConvert.SerializeObject(doc.Data);
            return JsonConvert.DeserializeObject<Vanguard_Engine.Entities.Role>(json);
        }
        catch
        {
            return null;
        }
    }
}
