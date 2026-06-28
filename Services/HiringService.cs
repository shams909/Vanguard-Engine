using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class HiringService : IHiringService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public HiringService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<List<HiringNotice>> GetAllAsync()
        => await _unitOfWork.HiringNotices.GetAllAsync();

    public async Task<List<HiringNotice>> GetOpenNoticesAsync()
        => await _unitOfWork.HiringNotices.GetOpenNoticesAsync();

    public async Task<HiringNotice?> GetByIdAsync(string id)
        => await _unitOfWork.HiringNotices.GetByIdAsync(id);

    public async Task<List<HiringNotice>> GetExpiredAsync()
    {
        var all = await _unitOfWork.HiringNotices.GetAllAsync();
        return all
            .Where(n => n.Status == "Open" && n.ExpiryDate.HasValue && n.ExpiryDate.Value < DateTime.UtcNow)
            .ToList();
    }

    public async Task<(bool Success, string Error)> CreateAsync(HiringNotice notice)
    {
        if (string.IsNullOrWhiteSpace(notice.Title) || string.IsNullOrWhiteSpace(notice.Description))
            return (false, "Title and Description are required.");

        if (notice.NumberOfPositions < 1)
            return (false, "At least 1 position must be available.");

        notice.CreatedAt = DateTime.UtcNow;
        notice.Status = "Open";
        notice.FilledPositions = 0;

        // Auto-generate reference code if not set
        if (string.IsNullOrWhiteSpace(notice.ReferenceCode))
            notice.ReferenceCode = "V-REQ-" + DateTime.UtcNow.ToString("yyMMdd") +
                                   System.Security.Cryptography.RandomNumberGenerator.GetInt32(100, 999);

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
        if (existing.Status == "Closed" || existing.Status == "Filled")
            return (false, $"Notice is already '{existing.Status}'.");

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

    public async Task<(bool Success, string Error)> FillPositionAsync(string noticeId)
    {
        var notice = await _unitOfWork.HiringNotices.GetByIdAsync(noticeId);
        if (notice == null) return (false, "Hiring notice not found.");
        if (notice.Status != "Open") return (false, $"Notice is '{notice.Status}' and not accepting new positions.");

        notice.FilledPositions += 1;

        if (notice.IsFull)
        {
            // Auto-close the notice when fully staffed
            await _unitOfWork.HiringNotices.UpdateStatusAsync(noticeId, "Filled");

            // Notify the recruiter who posted the notice
            if (!string.IsNullOrEmpty(notice.PostedByUserId))
            {
                await _notificationService.CreateNotificationAsync(
                    notice.PostedByUserId,
                    "Hiring Notice Fully Staffed",
                    $"Hiring notice '{notice.Title}' ({notice.ReferenceCode}) has been fully staffed ({notice.NumberOfPositions}/{notice.NumberOfPositions} positions filled).",
                    "Info");
            }
        }
        else
        {
            // Update the filled count without closing
            await _unitOfWork.HiringNotices.UpdateFilledPositionsAsync(noticeId, notice.FilledPositions);
        }

        return (true, string.Empty);
    }
}

