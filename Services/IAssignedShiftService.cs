using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IAssignedShiftService
{
    Task<(bool Success, string Error)> AssignShiftAsync(string guardId, string guardName, string shiftDate, string startTime, string endTime);
    Task<List<AssignedShift>> GetGuardScheduleAsync(string guardId);
    Task<List<AssignedShift>> GetAllAssignedShiftsAsync();
    Task<(bool Success, string Error)> UpdateShiftStatusAsync(string shiftId, string status);
}
