using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Repositories;

public interface IVIPRequestRepository : IGenericRepository<VIPRequest>
{
    Task<List<VIPRequest>> GetAllAsync();
    Task<List<VIPRequest>> GetByClientIdAsync(string clientId);
    Task<List<VIPRequest>> GetByStatusAsync(string status);
    Task UpdateStatusAsync(string id, string status);
    Task UpdateAssignedGuardsAsync(string id, List<string> guardIds);
    Task DeleteAsync(string id);
}
