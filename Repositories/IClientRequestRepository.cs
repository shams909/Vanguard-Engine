using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Repositories;

public interface IClientRequestRepository : IGenericRepository<ClientRequest>
{
    Task<List<ClientRequest>> GetAllAsync();
    Task<List<ClientRequest>> GetByClientIdAsync(string clientId);
    Task<List<ClientRequest>> GetByStatusAsync(string status);
    Task UpdateAsync(ClientRequest request);
    Task UpdateStatusAsync(string id, string status);
    Task UpdateAssignedGuardsAsync(string id, List<string> guardIds);
    Task DeleteAsync(string id);
}
