using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IVipApplicationService
{
    Task<(bool Success, string Error)> ApplyAsync(VipApplication application);
    Task<VipApplication?> GetMyPendingApplicationAsync(string clientId);
    Task<List<VipApplication>> GetMyApplicationsAsync(string clientId);
    Task<List<VipApplication>> GetAllApplicationsAsync();
    Task<List<VipApplication>> GetPendingApplicationsAsync();
    Task<(bool Success, string Error)> ApproveAsync(string applicationId);
    Task<(bool Success, string Error)> RejectAsync(string applicationId);
}
