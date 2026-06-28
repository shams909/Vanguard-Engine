using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IAssignedShiftService
{
    // ── Queries ───────────────────────────────────────────────────────────────
    Task<List<AssignedShift>> GetGuardScheduleAsync(string guardId);
    Task<List<AssignedShift>> GetAllAssignedShiftsAsync();
    Task<List<AssignedShift>> GetByClientRequestIdAsync(string clientRequestId);

    // ── Commands ──────────────────────────────────────────────────────────────
    Task<(bool Success, string Error)> AssignShiftAsync(
        string guardId, string guardName,
        string shiftDate, string startTime, string endTime,
        string? clientRequestId = null, string? location = null, string? notes = null);

    Task<(bool Success, string Error)> UpdateShiftStatusAsync(string shiftId, string status);

    /// <summary>
    /// MODULE 8: Admin force-cancels a shift when a guard fails to report.
    /// Releases the guard back to Available and notifies all parties.
    /// </summary>
    Task<(bool Success, string Error)> ForceCheckoutAsync(string shiftId, string adminId);
}
