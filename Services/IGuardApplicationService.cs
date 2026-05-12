using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IGuardApplicationService
{
    Task<bool> ApplyAsync(string userId, string experience, string skills);
    Task<List<GuardApplication>> GetMyApplicationsAsync(string userId);
    Task<List<GuardApplication>> GetAllApplicationsAsync();
    Task<bool> ApproveAsync(string id);
    Task<bool> RejectAsync(string id);
    Task<bool> DeleteAsync(string id);
}
