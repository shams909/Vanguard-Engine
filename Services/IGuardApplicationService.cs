using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IGuardApplicationService
{
    Task<(bool Success, string Error)> ApplyAsync(string userId, GuardApplication application);
    Task<List<GuardApplication>> GetMyApplicationsAsync(string userId);
    Task<GuardApplication?> GetApplicationByIdAsync(string id, string userId);
    Task<List<GuardApplication>> GetAllApplicationsAsync();
    Task<GuardApplication?> GetByIdAsync(string id);
    Task<(bool Success, string Error)> ApproveAsync(string id);
    Task<(bool Success, string Error)> RejectAsync(string id);
    Task<(bool Success, string Error)> DeleteAsync(string id, string userId);

    // recruitment workflow
    Task<(bool Success, string Error)> ApplyToJobAsync(string guardUserId, string requestId);
    Task<List<GuardApplication>> GetApplicationsForJobAsync(string jobId);
    Task<(bool Success, string Error)> AcceptJobApplicationAsync(string applicationId, string clientId);
    Task<(bool Success, string Error)> RejectJobApplicationAsync(string applicationId, string clientId);
    Task<(bool Success, string Error)> CompleteJobAsync(string requestId);
}
