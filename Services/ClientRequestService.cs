using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class ClientRequestService : IClientRequestService
{
    private readonly IUnitOfWork _unitOfWork;

    public ClientRequestService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ClientRequest>> GetAllRequestsAsync()
    {
        return await _unitOfWork.ClientRequests.GetAllAsync();
    }

    public async Task<List<ClientRequest>> GetRequestsByClientAsync(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId)) return new List<ClientRequest>();
        return await _unitOfWork.ClientRequests.GetByClientIdAsync(clientId);
    }

    public async Task<List<ClientRequest>> GetRequestsByStatusAsync(string status)
    {
        if (string.IsNullOrWhiteSpace(status)) return new List<ClientRequest>();
        return await _unitOfWork.ClientRequests.GetByStatusAsync(status);
    }

    public async Task<ClientRequest?> GetRequestByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return await _unitOfWork.ClientRequests.GetByIdAsync(id);
    }

    public async Task<(bool Success, string Error)> CreateRequestAsync(ClientRequest request)
    {
        // STEP 3 Validation Handling
        if (request == null)
            return (false, "Request payload cannot be empty.");

        if (string.IsNullOrWhiteSpace(request.ClientId))
            return (false, "Client identity is required.");

        if (request.NumberOfGuards <= 0)
            return (false, "You must request at least one security guard.");

        if (string.IsNullOrWhiteSpace(request.Location))
            return (false, "Patrol location is required.");

        if (string.IsNullOrWhiteSpace(request.Duration))
            return (false, "Shift duration is required.");

        request.Status = "Pending";

        await _unitOfWork.ClientRequests.AddAsync(request);
        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> UpdateRequestStatusAsync(string id, string status)
    {
        var existing = await _unitOfWork.ClientRequests.GetByIdAsync(id);
        if (existing == null)
            return (false, "Client security request not found.");

        await _unitOfWork.ClientRequests.UpdateStatusAsync(id, status);
        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> AssignGuardsToRequestAsync(string id, List<string> guardIds)
    {
        var existing = await _unitOfWork.ClientRequests.GetByIdAsync(id);
        if (existing == null)
            return (false, "Client security request not found.");

        await _unitOfWork.ClientRequests.UpdateAssignedGuardsAsync(id, guardIds);
        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DeleteRequestAsync(string id)
    {
        var existing = await _unitOfWork.ClientRequests.GetByIdAsync(id);
        if (existing == null)
            return (false, "Client security request not found.");

        await _unitOfWork.ClientRequests.DeleteAsync(id);
        return (true, string.Empty);
    }
}
