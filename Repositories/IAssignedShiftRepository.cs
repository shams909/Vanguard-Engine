using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Repositories;

public interface IAssignedShiftRepository : IGenericRepository<AssignedShift>
{
    Task<List<AssignedShift>> GetByGuardIdAsync(string guardId);
    Task<List<AssignedShift>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<List<AssignedShift>> GetByStatusAsync(string status);
    Task<List<AssignedShift>> GetAllAssignedShiftsAsync();

    /// <summary>MODULE 8: Returns all shifts linked to a specific ClientRequest deployment.</summary>
    Task<List<AssignedShift>> GetByClientRequestIdAsync(string clientRequestId);

    /// <summary>MODULE 8: Atomically updates just the status field.</summary>
    Task UpdateStatusAsync(string id, string status);

    Task DeleteAsync(string id);
}

