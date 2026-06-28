using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class GuardApplicationService : IGuardApplicationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLog;

    public GuardApplicationService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IAuditLogService auditLog)
    {
        _unitOfWork          = unitOfWork;
        _notificationService = notificationService;
        _auditLog            = auditLog;
    }

    // ── General Guard Registration ────────────────────────────────────────────

    public async Task<(bool Success, string Error)> ApplyAsync(string userId, GuardApplication application)
    {
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

        var all = await _unitOfWork.GuardApplications.GetAllAsync();
        var userApps = all.Where(a => a.UserId == userId && string.IsNullOrEmpty(a.JobId)).ToList();

        var hasActive = userApps.Any(a => a.Status == "Pending" || a.Status == "Approved");
        if (hasActive)
        {
            var activeApp = userApps.First(a => a.Status == "Pending" || a.Status == "Approved");
            return (false, $"You already have a {activeApp.Status.ToLower()} application. You may only re-apply after it is rejected.");
        }

        application.UserId = userId;
        application.Status = "Pending";
        await _unitOfWork.GuardApplications.AddAsync(application);

        // MODULE 11: Audit trail
        await _auditLog.LogAsync("GuardApplication", application.Id, "Created",
            userId, toValue: "Pending", performedByRole: "Guard");

        await _notificationService.CreateNotificationAsync(
            userId,
            "Guard Application Submitted",
            "Your guard application has been received and is under review by our team.",
            "Info");

        // Notify Recruiter — recruiters handle guard onboarding
        await _notificationService.NotifyRoleAsync(
            "Recruiter",
            "New Guard Application Received",
            "A new guard application has been submitted. Please review it in the Recruiter Panel.",
            "Info");

        return (true, string.Empty);
    }

    public async Task<List<GuardApplication>> GetMyApplicationsAsync(string userId)
    {
        var all = await _unitOfWork.GuardApplications.GetAllAsync();
        return all.Where(a => a.UserId == userId).OrderByDescending(a => a.CreatedAt).ToList();
    }

    public async Task<List<GuardApplication>> GetAllApplicationsAsync()
        => await _unitOfWork.GuardApplications.GetAllAsync();

    public async Task<GuardApplication?> GetApplicationByIdAsync(string id, string userId)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        var app = await _unitOfWork.GuardApplications.GetByIdAsync(id);
        return (app != null && app.UserId == userId) ? app : null;
    }

    public async Task<GuardApplication?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return await _unitOfWork.GuardApplications.GetByIdAsync(id);
    }

    // ── Admin / Recruiter — General Application Review ────────────────────────

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

        // MODULE 1: Set guard operational status on User entity
        if (!string.IsNullOrEmpty(application.UserId))
            await _unitOfWork.Users.UpdateGuardStatusAsync(application.UserId, "Available");

        // MODULE 11: Audit trail
        await _auditLog.LogAsync("GuardApplication", id, "StatusChanged",
            "system", fromValue: "Pending", toValue: "Approved",
            notes: $"Guard {application.FullName} entered available roster",
            performedByRole: "Recruiter");

        await _notificationService.CreateNotificationAsync(
            application.UserId,
            "Guard Application Approved!",
            "Congratulations! Your guard application has been approved. You are now part of our active guard roster.",
            "Info");

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

        // MODULE 11: Audit trail
        await _auditLog.LogAsync("GuardApplication", id, "StatusChanged",
            "system", fromValue: "Pending", toValue: "Rejected", performedByRole: "Recruiter");

        await _notificationService.CreateNotificationAsync(
            application.UserId,
            "Guard Application Not Approved",
            "Unfortunately your guard application was not approved at this time. You may re-apply in the future.",
            "Warning");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> DeleteAsync(string id, string userId)
    {
        if (string.IsNullOrWhiteSpace(id))
            return (false, "Invalid application ID.");

        var application = await _unitOfWork.GuardApplications.GetByIdAsync(id);
        if (application == null)
            return (false, "Application not found.");

        if (application.UserId != userId)
            return (false, "Unauthorized: You can only delete your own applications.");

        if (application.Status != "Pending")
            return (false, "You can only delete applications that are still Pending.");

        await _unitOfWork.GuardApplications.DeleteAsync(id);
        return (true, string.Empty);
    }

    // ── Guard — Apply to a ClientRequest Deployment ───────────────────────────

    public async Task<(bool Success, string Error)> ApplyToJobAsync(string guardUserId, string requestId)
    {
        if (string.IsNullOrWhiteSpace(guardUserId))
            return (false, "Session invalid.");

        // MODULE 1: Check availability directly from User.GuardStatus
        var guardUser = await _unitOfWork.Users.GetByIdAsync(guardUserId);
        if (guardUser == null)
            return (false, "Guard account not found.");

        if (guardUser.GuardStatus != "Available")
            return (false, $"You are currently {guardUser.GuardStatus ?? "not eligible"} and cannot apply for new deployments.");

        // Guard must have an approved general application (be in the roster)
        var allApps = await _unitOfWork.GuardApplications.GetAllAsync();
        var generalProfile = allApps.FirstOrDefault(a =>
            a.UserId == guardUserId && string.IsNullOrEmpty(a.JobId) && a.Status == "Approved");
        if (generalProfile == null)
            return (false, "You must have an approved Guard registration to apply for deployments.");

        // Duplicate application check
        var hasApplied = allApps.Any(a => a.UserId == guardUserId && a.JobId == requestId);
        if (hasApplied)
            return (false, "You have already applied to this recruitment post.");

        var request = await _unitOfWork.ClientRequests.GetByIdAsync(requestId);
        if (request == null)
            return (false, "Deployment post not found.");

        if (request.Status != "Approved" && request.Status != "Open" && request.Status != "Partially Assigned")
            return (false, "This recruitment post is currently closed.");

        if (request.AssignedGuardIds != null && request.AssignedGuardIds.Count >= request.NumberOfGuards)
            return (false, "This recruitment post's guard requirements are already fully met.");

        var jobApp = new GuardApplication
        {
            UserId            = guardUserId,
            JobId             = requestId,
            FullName          = generalProfile.FullName,
            Phone             = generalProfile.Phone,
            NationalId        = generalProfile.NationalId,
            Address           = generalProfile.Address,
            YearsOfExperience = generalProfile.YearsOfExperience,
            Experience        = generalProfile.Experience,
            Skills            = generalProfile.Skills,
            PreferredLocation = generalProfile.PreferredLocation,
            ArmedLicense      = generalProfile.ArmedLicense,
            Status            = "Pending",
        };

        await _unitOfWork.GuardApplications.AddAsync(jobApp);

        await _notificationService.CreateNotificationAsync(
            guardUserId,
            "Job Application Submitted",
            $"Your application for deployment at '{request.Location}' has been submitted.",
            "Info");

        await _notificationService.CreateNotificationAsync(
            request.ClientId,
            "New Guard Applied to Your Post",
            $"A guard has applied to your deployment request at '{request.Location}'. Review their application.",
            "Info");

        return (true, string.Empty);
    }

    public async Task<List<GuardApplication>> GetApplicationsForJobAsync(string jobId)
        => await _unitOfWork.GuardApplications.GetByJobIdAsync(jobId);

    // ── Client — Shortlist / Accept / Reject Guard Applications ──────────────

    public async Task<(bool Success, string Error)> ShortlistJobApplicationAsync(string applicationId, string clientId)
    {
        var application = await _unitOfWork.GuardApplications.GetByIdAsync(applicationId);
        if (application == null)
            return (false, "Application not found.");

        if (string.IsNullOrEmpty(application.JobId))
            return (false, "This is not a deployment application.");

        var request = await _unitOfWork.ClientRequests.GetByIdAsync(application.JobId);
        if (request == null)
            return (false, "Deployment post not found.");

        if (request.ClientId != clientId)
            return (false, "Unauthorized: You can only manage applications on your own requests.");

        if (application.Status != "Pending")
            return (false, $"Only Pending applications can be shortlisted. Current status: '{application.Status}'.");

        await _unitOfWork.GuardApplications.UpdateStatusAsync(applicationId, "Shortlisted");

        await _notificationService.CreateNotificationAsync(
            application.UserId,
            "Application Shortlisted",
            $"Your application for the deployment at '{request.Location}' has been shortlisted. Await final decision.",
            "Info");

        return (true, string.Empty);
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
            return (false, "Deployment post not found.");

        if (request.ClientId != clientId)
            return (false, "Unauthorized: You can only manage applications on your own requests.");

        if (application.Status != "Pending" && application.Status != "Shortlisted")
            return (false, $"Application is already '{application.Status}'.");

        // Guard count enforcement — prevent over-assignment
        var currentCount = request.AssignedGuardIds?.Count ?? 0;
        if (currentCount >= request.NumberOfGuards)
            return (false, $"This request already has the required {request.NumberOfGuards} guard(s) assigned.");

        // MODULE 1: Check guard availability from User entity — not GuardApplication
        var guardUser = await _unitOfWork.Users.GetByIdAsync(application.UserId);
        if (guardUser == null)
            return (false, "Guard account not found.");

        if (guardUser.GuardStatus == "Assigned" || guardUser.GuardStatus == "OnDuty")
            return (false, "This officer is already on an active deployment.");

        if (guardUser.GuardStatus == "Suspended")
            return (false, "This officer is currently suspended and cannot be assigned.");

        // Accept application and set guard status to Assigned
        await _unitOfWork.GuardApplications.UpdateStatusAsync(applicationId, "Accepted");
        await _unitOfWork.Users.UpdateGuardStatusAsync(application.UserId, "Assigned");

        // Add guard to request's assigned list
        if (request.AssignedGuardIds == null)
            request.AssignedGuardIds = new List<string>();
        if (!request.AssignedGuardIds.Contains(application.UserId))
            request.AssignedGuardIds.Add(application.UserId);
        await _unitOfWork.ClientRequests.UpdateAssignedGuardsAsync(request.Id, request.AssignedGuardIds);

        // Determine new request status based on guard count
        var newCount = request.AssignedGuardIds.Count;
        if (newCount >= request.NumberOfGuards)
        {
            await _unitOfWork.ClientRequests.UpdateStatusAsync(request.Id, "Assigned");
            await AutoRejectRemainingApplicationsAsync(request.Id, applicationId, request.Location);

            await _notificationService.CreateNotificationAsync(
                request.ClientId,
                "All Guards Assigned",
                $"Your request at '{request.Location}' is now fully staffed with {request.NumberOfGuards} guard(s). Ready to schedule.",
                "Info");
        }
        else
        {
            await _unitOfWork.ClientRequests.UpdateStatusAsync(request.Id, "Partially Assigned");

            await _notificationService.CreateNotificationAsync(
                request.ClientId,
                "Guard Application Accepted",
                $"A guard has been accepted for your request at '{request.Location}'. {request.NumberOfGuards - newCount} more guard(s) needed.",
                "Info");
        }

        await _notificationService.CreateNotificationAsync(
            application.UserId,
            "Job Application Accepted!",
            $"Your application for the deployment at '{request.Location}' has been accepted. You are now assigned.",
            "Info");

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
            return (false, "Deployment post not found.");

        if (request.ClientId != clientId)
            return (false, "Unauthorized: You can only manage applications on your own requests.");

        if (application.Status != "Pending" && application.Status != "Shortlisted")
            return (false, $"Application is already '{application.Status}'.");

        await _unitOfWork.GuardApplications.UpdateStatusAsync(applicationId, "Rejected");

        await _notificationService.CreateNotificationAsync(
            application.UserId,
            "Job Application Not Accepted",
            $"Your application for the deployment at '{request.Location}' was not accepted.",
            "Warning");

        return (true, string.Empty);
    }

    // ── Admin / Recruiter — Complete a Deployment Contract ───────────────────

    public async Task<(bool Success, string Error)> CompleteJobAsync(string requestId)
    {
        var request = await _unitOfWork.ClientRequests.GetByIdAsync(requestId);
        if (request == null)
            return (false, "Deployment post not found.");

        await _unitOfWork.ClientRequests.UpdateStatusAsync(requestId, "Completed");

        // MODULE 1: Release all assigned guards via User.GuardStatus — no more GuardApplication.GuardStatus
        if (request.AssignedGuardIds != null && request.AssignedGuardIds.Any())
        {
            foreach (var guardId in request.AssignedGuardIds)
            {
                await _unitOfWork.Users.UpdateGuardStatusAsync(guardId, "Available");

                await _notificationService.CreateNotificationAsync(
                    guardId,
                    "Deployment Completed",
                    $"Your deployment at '{request.Location}' has been completed. You are now available.",
                    "Info");
            }
        }

        await _notificationService.CreateNotificationAsync(
            request.ClientId,
            "Deployment Contract Completed",
            $"Your security deployment at '{request.Location}' has been marked as completed. Thank you.",
            "Info");

        return (true, string.Empty);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// When a request reaches full guard capacity, auto-rejects all remaining
    /// Pending and Shortlisted applications and notifies each rejected guard.
    /// </summary>
    private async Task AutoRejectRemainingApplicationsAsync(string requestId, string acceptedApplicationId, string location)
    {
        var allApps = await _unitOfWork.GuardApplications.GetByJobIdAsync(requestId);
        var toReject = allApps.Where(a =>
            a.Id != acceptedApplicationId &&
            (a.Status == "Pending" || a.Status == "Shortlisted")).ToList();

        foreach (var app in toReject)
        {
            await _unitOfWork.GuardApplications.UpdateStatusAsync(app.Id, "Rejected");

            await _notificationService.CreateNotificationAsync(
                app.UserId,
                "Position Filled",
                $"The deployment post at '{location}' has been fully staffed and your application has been closed.",
                "Warning");
        }
    }
}
