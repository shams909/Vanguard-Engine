using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class GuardShiftService : IGuardShiftService
{
    private readonly IUnitOfWork _unitOfWork;

    public GuardShiftService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ── Guard Operations ──────────────────────────────────────────────────

    public async Task<(bool Success, string Error)> CheckInAsync(string guardId, string guardName)
    {
        if (string.IsNullOrWhiteSpace(guardId))
            return (false, "Session is invalid. Please log in again.");

        // Prevent double check-in
        var existing = await _unitOfWork.GuardShifts.GetActiveShiftAsync(guardId);
        if (existing != null)
            return (false, "You already have an active shift. Check out first before starting a new one.");

        var shift = new GuardShift
        {
            GuardId      = guardId,
            GuardName    = guardName,
            CheckInTime  = DateTime.UtcNow.ToString("o"), // ISO 8601 round-trip
            CheckOutTime = null,
            DurationMinutes = 0,
            Status       = "Active"
        };

        await _unitOfWork.GuardShifts.AddAsync(shift);
        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> CheckOutAsync(string shiftId, string guardId)
    {
        if (string.IsNullOrWhiteSpace(shiftId))
            return (false, "Shift ID is required.");

        var shift = await _unitOfWork.GuardShifts.GetByIdAsync(shiftId);
        if (shift == null)
            return (false, "Shift record not found.");

        // Ownership check — guard can only check out their own shift
        if (shift.GuardId != guardId)
            return (false, "Unauthorized: You can only check out your own shift.");

        if (shift.Status != "Active")
            return (false, "This shift is already completed.");

        // Calculate duration
        var checkIn  = DateTime.Parse(shift.CheckInTime, null,
                           System.Globalization.DateTimeStyles.RoundtripKind);
        var checkOut = DateTime.UtcNow;
        var duration = (int)(checkOut - checkIn).TotalMinutes;

        await _unitOfWork.GuardShifts.UpdateCheckOutAsync(
            shiftId,
            checkOut.ToString("o"),
            duration
        );

        return (true, string.Empty);
    }

    public async Task<GuardShift?> GetActiveShiftAsync(string guardId)
    {
        if (string.IsNullOrWhiteSpace(guardId)) return null;
        return await _unitOfWork.GuardShifts.GetActiveShiftAsync(guardId);
    }

    public async Task<List<GuardShift>> GetShiftHistoryAsync(string guardId)
    {
        if (string.IsNullOrWhiteSpace(guardId)) return new();
        return await _unitOfWork.GuardShifts.GetByGuardIdAsync(guardId);
    }

    // ── Admin Operations ──────────────────────────────────────────────────

    public async Task<List<GuardShift>> GetAllShiftsAsync() =>
        await _unitOfWork.GuardShifts.GetAllAsync();

    public async Task<List<GuardShift>> GetActiveShiftsAsync() =>
        await _unitOfWork.GuardShifts.GetByStatusAsync("Active");

    public async Task<List<GuardShift>> GetCompletedShiftsAsync() =>
        await _unitOfWork.GuardShifts.GetByStatusAsync("Completed");

    // ── Stats ─────────────────────────────────────────────────────────────

    public async Task<(int Active, int CompletedToday, double AverageDurationMinutes)> GetTodayStatsAsync()
    {
        var all = await _unitOfWork.GuardShifts.GetAllAsync();
        var today = DateTime.UtcNow.Date;

        var active = all.Count(s => s.Status == "Active");

        var completedToday = all
            .Where(s => s.Status == "Completed" && s.CheckInDateTime.Date == today)
            .ToList();

        var avg = completedToday.Any()
            ? completedToday.Average(s => s.DurationMinutes)
            : 0;

        return (active, completedToday.Count, Math.Round(avg, 1));
    }
}
