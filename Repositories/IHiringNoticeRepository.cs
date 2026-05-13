using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Repositories;

public interface IHiringNoticeRepository : IGenericRepository<HiringNotice>
{
    Task<List<HiringNotice>> GetAllAsync();
    Task<List<HiringNotice>> GetOpenNoticesAsync();
    Task UpdateAsync(HiringNotice notice);
    Task UpdateStatusAsync(string id, string status);
    Task DeleteAsync(string id);
}
