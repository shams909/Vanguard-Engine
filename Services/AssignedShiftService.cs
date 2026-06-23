using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class AssignedShiftService : IAssignedShiftService
{
    private readonly IUnitOfWork _unitOfWork;

    public AssignedShiftService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool Success, string Error)> AssignShiftAsync(string guardId, string guardName, string shiftDate, string startTime, string endTime)
    {
        // Validation
        if (string.Compare(startTime, endTime) >= 0)
            return (false, "Start time must be before end time.");

        var existingShifts = await _unitOfWork.AssignedShifts.GetByGuardIdAsync(guardId);
        
        // Check for overlaps on the same date
        foreach (var shift in existingShifts.Where(s => s.ShiftDate == shiftDate && s.Status == "Scheduled"))
        {
            if (string.Compare(startTime, shift.EndTime) < 0 && string.Compare(endTime, shift.StartTime) > 0)
            {
                return (false, $"Overlap detected with existing shift: {shift.StartTime} - {shift.EndTime}");
            }
        }

        var newShift = new AssignedShift
        {
            GuardId = guardId,
            GuardName = guardName,
            ShiftDate = shiftDate,
            StartTime = startTime,
            EndTime = endTime,
            Status = "Scheduled"
        };

        await _unitOfWork.AssignedShifts.AddAsync(newShift);
        return (true, string.Empty);
    }

    public async Task<List<AssignedShift>> GetGuardScheduleAsync(string guardId)
    {
        if (string.IsNullOrWhiteSpace(guardId)) return new();
        return await _unitOfWork.AssignedShifts.GetByGuardIdAsync(guardId);
    }

    public async Task<List<AssignedShift>> GetAllAssignedShiftsAsync()
    {
        return await _unitOfWork.AssignedShifts.GetAllAssignedShiftsAsync();
    }

    public async Task<(bool Success, string Error)> UpdateShiftStatusAsync(string shiftId, string status)
    {
        var shift = await _unitOfWork.AssignedShifts.GetByIdAsync(shiftId);
        if (shift == null) return (false, "Shift not found.");

        shift.Status = status;
        _unitOfWork.AssignedShifts.Update(shift);
        return (true, string.Empty);
    }
}
