using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IHiringService
{
    Task<List<HiringNotice>> GetAllAsync();
    Task<List<HiringNotice>> GetOpenNoticesAsync();
    Task<HiringNotice?> GetByIdAsync(string id);
    Task<(bool Success, string Error)> CreateAsync(HiringNotice notice);
    Task<(bool Success, string Error)> UpdateAsync(HiringNotice notice);
    Task<(bool Success, string Error)> CloseNoticeAsync(string id);
    Task<(bool Success, string Error)> DeleteAsync(string id);
}
