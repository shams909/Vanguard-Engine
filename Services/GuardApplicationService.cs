using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class GuardApplicationService : IGuardApplicationService
{
    private readonly IUnitOfWork _unitOfWork;

    public GuardApplicationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool Success, string Error)> ApplyAsync(string userId, GuardApplication application)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(userId))
            return (false, "User session is invalid. Please log in again.");

        if (string.IsNullOrWhiteSpace(application.FullName) ||
            string.IsNullOrWhiteSpace(application.Phone) ||
            string.IsNullOrWhiteSpace(application.NationalId) ||
            string.IsNullOrWhiteSpace(application.Address) ||
            string.IsNullOrWhiteSpace(application.Experience) ||
            string.IsNullOrWhiteSpace(application.Skills) ||
            string.IsNullOrWhiteSpace(application.PreferredLocation))
        {
            return (false, "All required fields must be filled in.");
        }

        // Check for existing active applications
        // Get all user's applications and check for Pending or Approved
        var all = await _unitOfWork.GuardApplications.GetAllAsync();
        var userApps = all.Where(a => a.UserId == userId).ToList();

        var hasActive = userApps.Any(a => a.Status == "Pending" || a.Status == "Approved");
        if (hasActive)
        {
            var activeApp = userApps.First(a => a.Status == "Pending" || a.Status == "Approved");
            return (false, $"You already have a {activeApp.Status.ToLower()} application. You may only re-apply after it is rejected.");
        }

        // Build the entity
        application.UserId = userId;
        application.Status = "Pending";

        await _unitOfWork.GuardApplications.AddAsync(application);
        return (true, string.Empty);
    }

    public async Task<List<GuardApplication>> GetMyApplicationsAsync(string userId)
    {
        var all = await _unitOfWork.GuardApplications.GetAllAsync();
        return all.Where(a => a.UserId == userId).OrderByDescending(a => a.CreatedAt).ToList();
    }

    public async Task<List<GuardApplication>> GetAllApplicationsAsync()
    {
        return await _unitOfWork.GuardApplications.GetAllAsync();
    }

    public async Task<GuardApplication?> GetApplicationByIdAsync(string id, string userId)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        var app = await _unitOfWork.GuardApplications.GetByIdAsync(id);
        // Only return if it belongs to this user (security check)
        return (app != null && app.UserId == userId) ? app : null;
    }

    public async Task<GuardApplication?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return await _unitOfWork.GuardApplications.GetByIdAsync(id);
    }

    public async Task<(bool Success, string Error)> ApproveAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return (false, "Invalid application ID.");

        var application = await _unitOfWork.GuardApplications.GetByIdAsync(id);
        if (application == null)
            return (false, "Application not found.");

        if (application.Status != "Pending")
            return (false, $"Cannot approve an application with status '{application.Status}'. Only Pending applications can be approved.");

        await _unitOfWork.GuardApplications.UpdateStatusAsync(id, "Approved");
        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> RejectAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return (false, "Invalid application ID.");

        var application = await _unitOfWork.GuardApplications.GetByIdAsync(id);
        if (application == null)
            return (false, "Application not found.");

        if (application.Status != "Pending")
            return (false, $"Cannot reject an application with status '{application.Status}'. Only Pending applications can be rejected.");

        await _unitOfWork.GuardApplications.UpdateStatusAsync(id, "Rejected");
        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DeleteAsync(string id, string userId)
    {
        if (string.IsNullOrWhiteSpace(id))
            return (false, "Invalid application ID.");

        var application = await _unitOfWork.GuardApplications.GetByIdAsync(id);
        if (application == null)
            return (false, "Application not found.");

        // Ownership check — user can only delete their own application
        if (application.UserId != userId)
            return (false, "Unauthorized: You can only delete your own applications.");

        if (application.Status != "Pending")
            return (false, "You can only delete applications that are still Pending.");

        await _unitOfWork.GuardApplications.DeleteAsync(id);
        return (true, string.Empty);
    }
}
