using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IIncidentService
{
    Task<(bool Success, string Error)> SubmitIncidentAsync(string userId, string userName, string userRole, string type, string title, string description);
    Task<(bool Success, string Error)> ResolveIncidentAsync(string incidentId, string adminId, string resolutionNotes);
    Task<List<Incident>> GetMyReportsAsync(string userId);
    Task<List<Incident>> GetAllIncidentsAsync();
    Task<List<Incident>> GetIncidentsByStatusAsync(string status);
}
