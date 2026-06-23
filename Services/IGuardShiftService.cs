using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IGuardShiftService
{
    // ── Guard Operations ──────────────────────────────────────────────────
    Task<(bool Success, string Error)> CheckInAsync(string guardId, string guardName, string assignedShiftId);
    Task<(bool Success, string Error)> CheckOutAsync(string shiftId, string guardId);
    Task<GuardShift?> GetActiveShiftAsync(string guardId);
    Task<List<GuardShift>> GetShiftHistoryAsync(string guardId);

    // ── Admin Operations ──────────────────────────────────────────────────
    Task<List<GuardShift>> GetAllShiftsAsync();
    Task<List<GuardShift>> GetActiveShiftsAsync();
    Task<List<GuardShift>> GetCompletedShiftsAsync();

    // ── Stats ─────────────────────────────────────────────────────────────
    Task<(int Active, int CompletedToday, double AverageDurationMinutes)> GetTodayStatsAsync();
}
