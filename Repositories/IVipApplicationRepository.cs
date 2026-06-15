using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Repositories;

public interface IVipApplicationRepository : IGenericRepository<VipApplication>
{
    Task<List<VipApplication>> GetAllAsync();
    Task<List<VipApplication>> GetByClientIdAsync(string clientId);
    Task<VipApplication?> GetPendingApplicationAsync(string clientId);
    Task<List<VipApplication>> GetByStatusAsync(string status);
    Task UpdateStatusAsync(string id, string status);
}
