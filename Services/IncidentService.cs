using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class IncidentService : IIncidentService
{
    private readonly IUnitOfWork _unitOfWork;

    public IncidentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool Success, string Error)> SubmitIncidentAsync(string userId, string userName, string userRole, string type, string title, string description)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            return (false, "Please provide all required fields.");

        var incident = new Incident
        {
            ReportedByUserId = userId,
            ReportedByName = userName,
            ReporterRole = userRole, // Guard or Client
            Type = type,             // Incident or Complaint
            Title = title,
            Description = description,
            Status = "Open",
            ResolutionNotes = "",
            ResolvedByAdminId = ""
        };

        await _unitOfWork.Incidents.AddAsync(incident);
        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> ResolveIncidentAsync(string incidentId, string adminId, string resolutionNotes)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return (false, "Invalid incident ID.");
        if (string.IsNullOrWhiteSpace(resolutionNotes)) return (false, "Resolution notes are required.");

        var incident = await _unitOfWork.Incidents.GetByIdAsync(incidentId);
        if (incident == null) return (false, "Incident not found.");
        if (incident.Status == "Resolved") return (false, "This incident is already resolved.");

        incident.Status = "Resolved";
        incident.ResolutionNotes = resolutionNotes;
        incident.ResolvedByAdminId = adminId;
        incident.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Incidents.Update(incident);
        return (true, string.Empty);
    }

    public async Task<List<Incident>> GetMyReportsAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return new List<Incident>();
        return await _unitOfWork.Incidents.GetByReporterAsync(userId);
    }

    public async Task<List<Incident>> GetAllIncidentsAsync()
    {
        return await _unitOfWork.Incidents.GetAllIncidentsAsync();
    }

    public async Task<List<Incident>> GetIncidentsByStatusAsync(string status)
    {
        return await _unitOfWork.Incidents.GetByStatusAsync(status);
    }
}
