using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Repositories;

public interface IAssignedShiftRepository : IGenericRepository<AssignedShift>
{
    Task<List<AssignedShift>> GetByGuardIdAsync(string guardId);
    Task<List<AssignedShift>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<List<AssignedShift>> GetByStatusAsync(string status);
    Task<List<AssignedShift>> GetAllAssignedShiftsAsync();
}
