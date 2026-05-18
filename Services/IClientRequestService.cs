using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IClientRequestService
{
    Task<List<ClientRequest>> GetAllRequestsAsync();
    Task<List<ClientRequest>> GetRequestsByClientAsync(string clientId);
    Task<List<ClientRequest>> GetRequestsByStatusAsync(string status);
    Task<ClientRequest?> GetRequestByIdAsync(string id);
    Task<(bool Success, string Error)> CreateRequestAsync(ClientRequest request);
    Task<(bool Success, string Error)> UpdateRequestStatusAsync(string id, string status);
    Task<(bool Success, string Error)> AssignGuardsToRequestAsync(string id, List<string> guardIds);
    Task<(bool Success, string Error)> DeleteRequestAsync(string id);
}
