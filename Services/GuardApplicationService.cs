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

    public async Task<(bool Success, string Error)> ApplyToJobAsync(string guardUserId, string requestId)
    {
        if (string.IsNullOrWhiteSpace(guardUserId))
            return (false, "Session invalid.");

        // Check if guard has an approved profile (general application approved)
        var allApps = await _unitOfWork.GuardApplications.GetAllAsync();
        var generalProfile = allApps.FirstOrDefault(a => a.UserId == guardUserId && (string.IsNullOrEmpty(a.JobId) || a.JobId == ""));
        if (generalProfile == null || generalProfile.Status != "Approved")
        {
            return (false, "You must have an approved Guard registration to apply for deployments.");
        }

        // Check availability: Guard Status = Busy
        if (generalProfile.GuardStatus == "Busy")
        {
            return (false, "You already have an active assignment.");
        }

        // Prevent duplicate application: Guard cannot apply multiple times to the same recruitment post
        var hasApplied = allApps.Any(a => a.UserId == guardUserId && a.JobId == requestId);
        if (hasApplied)
        {
            return (false, "You have already applied to this recruitment post.");
        }

        // Check if deployment post exists and is active (Approved/Open)
        var request = await _unitOfWork.ClientRequests.GetByIdAsync(requestId);
        if (request == null)
        {
            return (false, "Deployment target post not found.");
        }

        // Support both "Approved" and "Open" statuses
        if (request.Status != "Approved" && request.Status != "Open")
        {
            return (false, "This recruitment post is currently closed.");
        }

        // Prevent applying if required guard count already filled
        if (request.AssignedGuardIds != null && request.AssignedGuardIds.Count >= request.NumberOfGuards)
        {
            return (false, "This recruitment post's guard requirements are already fully met.");
        }

        // Create job application request
        var jobApp = new GuardApplication
        {
            UserId = guardUserId,
            JobId = requestId,
            FullName = generalProfile.FullName,
            Phone = generalProfile.Phone,
            NationalId = generalProfile.NationalId,
            Address = generalProfile.Address,
            YearsOfExperience = generalProfile.YearsOfExperience,
            Experience = generalProfile.Experience,
            Skills = generalProfile.Skills,
            PreferredLocation = generalProfile.PreferredLocation,
            ArmedLicense = generalProfile.ArmedLicense,
            Status = "Pending", // Application is pending client review
            GuardStatus = "Available"
        };

        await _unitOfWork.GuardApplications.AddAsync(jobApp);
        return (true, string.Empty);
    }

    public async Task<List<GuardApplication>> GetApplicationsForJobAsync(string jobId)
    {
        return await _unitOfWork.GuardApplications.GetByJobIdAsync(jobId);
    }

    public async Task<(bool Success, string Error)> AcceptJobApplicationAsync(string applicationId, string clientId)
    {
        var application = await _unitOfWork.GuardApplications.GetByIdAsync(applicationId);
        if (application == null)
            return (false, "Application not found.");

        if (string.IsNullOrEmpty(application.JobId))
            return (false, "This is not a deployment job application.");

        var request = await _unitOfWork.ClientRequests.GetByIdAsync(application.JobId);
        if (request == null)
            return (false, "Deployment target post not found.");

        // Ownership check: must belong to the logged-in client
        if (request.ClientId != clientId)
            return (false, "Unauthorized request.");

        if (application.Status != "Pending")
            return (false, $"Application is already '{application.Status}'.");

        // Verify guard is still available before accepting
        var allApps = await _unitOfWork.GuardApplications.GetAllAsync();
        var generalProfile = allApps.FirstOrDefault(a => a.UserId == application.UserId && (string.IsNullOrEmpty(a.JobId) || a.JobId == ""));
        if (generalProfile == null || generalProfile.GuardStatus == "Busy")
        {
            return (false, "This officer has already been assigned to another active duty.");
        }

        // Accept application
        await _unitOfWork.GuardApplications.UpdateStatusAsync(applicationId, "Accepted");

        // Update Guard status: Available -> Busy (on both general and job profiles)
        await _unitOfWork.GuardApplications.UpdateGuardStatusAsync(applicationId, "Busy");
        if (generalProfile != null)
        {
            await _unitOfWork.GuardApplications.UpdateGuardStatusAsync(generalProfile.Id, "Busy");
        }

        // Assign guard to recruitment work: add guard's userId to client request's assigned list
        if (request.AssignedGuardIds == null)
        {
            request.AssignedGuardIds = new List<string>();
        }
        if (!request.AssignedGuardIds.Contains(application.UserId))
        {
            request.AssignedGuardIds.Add(application.UserId);
        }
        await _unitOfWork.ClientRequests.UpdateAssignedGuardsAsync(request.Id, request.AssignedGuardIds);

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> RejectJobApplicationAsync(string applicationId, string clientId)
    {
        var application = await _unitOfWork.GuardApplications.GetByIdAsync(applicationId);
        if (application == null)
            return (false, "Application not found.");

        if (string.IsNullOrEmpty(application.JobId))
            return (false, "This is not a deployment job application.");

        var request = await _unitOfWork.ClientRequests.GetByIdAsync(application.JobId);
        if (request == null)
            return (false, "Deployment target post not found.");

        // Ownership check
        if (request.ClientId != clientId)
            return (false, "Unauthorized request.");

        if (application.Status != "Pending")
            return (false, $"Application is already '{application.Status}'.");

        await _unitOfWork.GuardApplications.UpdateStatusAsync(applicationId, "Rejected");
        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> CompleteJobAsync(string requestId)
    {
        var request = await _unitOfWork.ClientRequests.GetByIdAsync(requestId);
        if (request == null)
            return (false, "Deployment target post not found.");

        // Complete the deployment request
        await _unitOfWork.ClientRequests.UpdateStatusAsync(requestId, "Completed");

        // Release the assigned guards: Busy -> Available
        if (request.AssignedGuardIds != null && request.AssignedGuardIds.Any())
        {
            var allApps = await _unitOfWork.GuardApplications.GetAllAsync();
            foreach (var guardId in request.AssignedGuardIds)
            {
                // Reset general profiles
                var generalProfile = allApps.FirstOrDefault(a => a.UserId == guardId && (string.IsNullOrEmpty(a.JobId) || a.JobId == ""));
                if (generalProfile != null)
                {
                    await _unitOfWork.GuardApplications.UpdateGuardStatusAsync(generalProfile.Id, "Available");
                }

                // Reset job application profiles
                var jobApps = allApps.Where(a => a.UserId == guardId && a.JobId == requestId).ToList();
                foreach (var ja in jobApps)
                {
                    await _unitOfWork.GuardApplications.UpdateGuardStatusAsync(ja.Id, "Available");
                }
            }
        }

        return (true, string.Empty);
    }
}
