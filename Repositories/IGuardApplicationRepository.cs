using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Repositories;

public interface IGuardApplicationRepository : IGenericRepository<GuardApplication>
{
    Task<GuardApplication?> GetByUserIdAsync(string userId);
    Task<List<GuardApplication>> GetAllAsync();
    Task UpdateAsync(GuardApplication application);
    Task UpdateStatusAsync(string id, string status);
    Task UpdateGuardStatusAsync(string id, string status);
    Task<List<GuardApplication>> GetByJobIdAsync(string jobId);
    Task DeleteAsync(string id);
}
