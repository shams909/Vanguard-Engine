using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class AssignedShiftService : IAssignedShiftService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public AssignedShiftService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<List<AssignedShift>> GetGuardScheduleAsync(string guardId)
    {
        if (string.IsNullOrWhiteSpace(guardId)) return new();
        return await _unitOfWork.AssignedShifts.GetByGuardIdAsync(guardId);
    }

    public async Task<List<AssignedShift>> GetAllAssignedShiftsAsync()
        => await _unitOfWork.AssignedShifts.GetAllAssignedShiftsAsync();

    public async Task<List<AssignedShift>> GetByClientRequestIdAsync(string clientRequestId)
    {
        if (string.IsNullOrWhiteSpace(clientRequestId)) return new();
        return await _unitOfWork.AssignedShifts.GetByClientRequestIdAsync(clientRequestId);
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public async Task<(bool Success, string Error)> AssignShiftAsync(
        string guardId, string guardName,
        string shiftDate, string startTime, string endTime,
        string? clientRequestId = null, string? location = null, string? notes = null)
    {
        // Time order validation
        if (string.Compare(startTime, endTime) >= 0)
            return (false, "Start time must be before end time.");

        // Guard availability check
        var guardUser = await _unitOfWork.Users.GetByIdAsync(guardId);
        if (guardUser == null) return (false, "Guard account not found.");
        if (guardUser.GuardStatus == "Suspended")
            return (false, "This officer is currently suspended and cannot be assigned to shifts.");

        // Overlap detection — same guard, same date, Scheduled or Active shift
        var existingShifts = await _unitOfWork.AssignedShifts.GetByGuardIdAsync(guardId);
        foreach (var shift in existingShifts.Where(s =>
            s.ShiftDate == shiftDate && (s.Status == "Scheduled" || s.Status == "Active")))
        {
            bool overlaps = string.Compare(startTime, shift.EndTime) < 0 &&
                            string.Compare(endTime, shift.StartTime) > 0;
            if (overlaps)
                return (false, $"Schedule conflict: guard already has a shift from {shift.StartTime}–{shift.EndTime} on {shiftDate}.");
        }

        var newShift = new AssignedShift
        {
            GuardId         = guardId,
            GuardName       = guardName,
            ShiftDate       = shiftDate,
            StartTime       = startTime,
            EndTime         = endTime,
            Status          = "Scheduled",
            ClientRequestId = clientRequestId,
            Location        = location,
            Notes           = notes
        };

        await _unitOfWork.AssignedShifts.AddAsync(newShift);

        // Notify the guard
        await _notificationService.CreateNotificationAsync(
            guardId,
            "New Shift Assigned",
            $"You have been assigned a shift on {shiftDate} from {startTime} to {endTime}" +
            (location != null ? $" at {location}" : "") + ".",
            "Info");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> UpdateShiftStatusAsync(string shiftId, string status)
    {
        var shift = await _unitOfWork.AssignedShifts.GetByIdAsync(shiftId);
        if (shift == null) return (false, "Shift not found.");

        // MODULE 8: Use proper async UpdateStatusAsync instead of fire-and-forget Update()
        await _unitOfWork.AssignedShifts.UpdateStatusAsync(shiftId, status);
        return (true, string.Empty);
    }

    /// <summary>
    /// MODULE 8: Admin force-cancels a shift (e.g. guard no-shows or is suspended mid-shift).
    /// Releases the guard back to Available and notifies guard + admin.
    /// </summary>
    public async Task<(bool Success, string Error)> ForceCheckoutAsync(string shiftId, string adminId)
    {
        var shift = await _unitOfWork.AssignedShifts.GetByIdAsync(shiftId);
        if (shift == null) return (false, "Shift not found.");

        if (shift.Status == "Completed" || shift.Status == "Cancelled")
            return (false, $"Shift is already '{shift.Status}' and cannot be force-cancelled.");

        await _unitOfWork.AssignedShifts.UpdateStatusAsync(shiftId, "Cancelled");

        // Release guard back to Available
        await _unitOfWork.Users.UpdateGuardStatusAsync(shift.GuardId, "Available");

        // Notify the guard
        await _notificationService.CreateNotificationAsync(
            shift.GuardId,
            "Shift Force-Cancelled",
            $"Your shift on {shift.ShiftDate} ({shift.StartTime}–{shift.EndTime}) has been cancelled by an administrator.",
            "Warning");

        // Notify admins
        await _notificationService.NotifyRoleAsync(
            "Admin",
            "Shift Force-Cancelled",
            $"Guard {shift.GuardName}'s shift on {shift.ShiftDate} was force-cancelled. Officer is now Available.",
            "Warning");

        return (true, string.Empty);
    }
}
