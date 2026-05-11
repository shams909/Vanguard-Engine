using Appwrite;
using Appwrite.Services;
using Role = Vanguard_Engine.Entities.Role;
using Vanguard_Engine.Services;
using Newtonsoft.Json;

namespace Vanguard_Engine.Repositories;

public class AppwriteRoleRepository : AppwriteRepository<Role>, IRoleRepository
{
    public AppwriteRoleRepository(IAppwriteService appwriteService) : base(appwriteService, "roles")
    {
    }

    public async Task<Role?> GetByNameAsync(string roleName)
    {
        var result = await _databases.ListDocuments(_databaseId, _collectionId, 
            queries: new List<string> { Query.Equal("roleName", roleName) });
        
        if (result.Total == 0) return null;
        
        return MapToEntity(result.Documents[0]);
    }
}
