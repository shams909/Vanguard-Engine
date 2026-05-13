using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class HiringService : IHiringService
{
    private readonly IUnitOfWork _unitOfWork;

    public HiringService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<HiringNotice>> GetAllAsync()
    {
        return await _unitOfWork.HiringNotices.GetAllAsync();
    }

    public async Task<List<HiringNotice>> GetOpenNoticesAsync()
    {
        return await _unitOfWork.HiringNotices.GetOpenNoticesAsync();
    }

    public async Task<HiringNotice?> GetByIdAsync(string id)
    {
        return await _unitOfWork.HiringNotices.GetByIdAsync(id);
    }

    public async Task<(bool Success, string Error)> CreateAsync(HiringNotice notice)
    {
        if (string.IsNullOrWhiteSpace(notice.Title) || string.IsNullOrWhiteSpace(notice.Description))
            return (false, "Title and Description are required.");

        notice.CreatedAt = DateTime.UtcNow;
        notice.Status = "Open";

        await _unitOfWork.HiringNotices.AddAsync(notice);
        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> UpdateAsync(HiringNotice notice)
    {
        var existing = await _unitOfWork.HiringNotices.GetByIdAsync(notice.Id);
        if (existing == null) return (false, "Hiring notice not found.");

        await _unitOfWork.HiringNotices.UpdateAsync(notice);
        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> CloseNoticeAsync(string id)
    {
        var existing = await _unitOfWork.HiringNotices.GetByIdAsync(id);
        if (existing == null) return (false, "Hiring notice not found.");

        await _unitOfWork.HiringNotices.UpdateStatusAsync(id, "Closed");
        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DeleteAsync(string id)
    {
        var existing = await _unitOfWork.HiringNotices.GetByIdAsync(id);
        if (existing == null) return (false, "Hiring notice not found.");

        await _unitOfWork.HiringNotices.DeleteAsync(id);
        return (true, string.Empty);
    }
}
