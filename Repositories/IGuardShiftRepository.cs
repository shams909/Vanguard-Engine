using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Repositories;

public interface IGuardShiftRepository : IGenericRepository<GuardShift>
{
    Task<List<GuardShift>> GetAllAsync();
    Task<List<GuardShift>> GetByGuardIdAsync(string guardId);
    Task<GuardShift?> GetActiveShiftAsync(string guardId);
    Task<List<GuardShift>> GetByStatusAsync(string status);
    Task UpdateCheckOutAsync(string id, string checkOutTime, int durationMinutes);
    Task UpdateStatusAsync(string id, string status);
    Task DeleteAsync(string id);
}
