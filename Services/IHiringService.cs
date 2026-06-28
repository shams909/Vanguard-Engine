using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IHiringService
{
    // ── Queries ───────────────────────────────────────────────────────────────
    Task<List<HiringNotice>> GetAllAsync();
    Task<List<HiringNotice>> GetOpenNoticesAsync();
    Task<HiringNotice?> GetByIdAsync(string id);
    Task<List<HiringNotice>> GetExpiredAsync();

    // ── Recruiter / Admin Operations ──────────────────────────────────────────
    Task<(bool Success, string Error)> CreateAsync(HiringNotice notice);
    Task<(bool Success, string Error)> UpdateAsync(HiringNotice notice);
    Task<(bool Success, string Error)> CloseNoticeAsync(string id);
    Task<(bool Success, string Error)> DeleteAsync(string id);

    /// <summary>
    /// MODULE 6: Increments FilledPositions by 1. If at capacity, auto-transitions Status to "Filled".
    /// Called by GuardApplicationService when a hiring-notice application is accepted.
    /// </summary>
    Task<(bool Success, string Error)> FillPositionAsync(string noticeId);
}

