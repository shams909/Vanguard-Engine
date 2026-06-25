using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Repositories;

public interface IIncidentRepository : IGenericRepository<Incident>
{
    Task<List<Incident>> GetByReporterAsync(string userId);
    Task<List<Incident>> GetByStatusAsync(string status);
    Task<List<Incident>> GetAllIncidentsAsync();
}
