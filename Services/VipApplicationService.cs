using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class VipApplicationService : IVipApplicationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly INotificationService _notificationService;

    public VipApplicationService(
        IUnitOfWork unitOfWork,
        IUserService userService,
        IRoleService roleService,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _userService = userService;
        _roleService = roleService;
        _notificationService = notificationService;
    }

    public async Task<(bool Success, string Error)> ApplyAsync(VipApplication application)
    {
        if (string.IsNullOrWhiteSpace(application.ClientId)) return (false, "Client ID is required.");
        if (string.IsNullOrWhiteSpace(application.VerificationDetails)) return (false, "Verification details are required.");

        var pending = await _unitOfWork.VipApplications.GetPendingApplicationAsync(application.ClientId);
        if (pending != null)
            return (false, "You already have a pending VIP application.");

        application.Status = "Pending";
        await _unitOfWork.VipApplications.AddAsync(application);

        // Notify the applicant
        await _notificationService.CreateNotificationAsync(
            application.ClientId,
            "VIP Application Submitted",
            "Your VIP application has been submitted and is under review.",
            "Info");

        return (true, string.Empty);
    }

    public async Task<VipApplication?> GetMyPendingApplicationAsync(string clientId)
        => await _unitOfWork.VipApplications.GetPendingApplicationAsync(clientId);

    public async Task<List<VipApplication>> GetMyApplicationsAsync(string clientId)
        => await _unitOfWork.VipApplications.GetByClientIdAsync(clientId);

    public async Task<List<VipApplication>> GetAllApplicationsAsync()
        => await _unitOfWork.VipApplications.GetAllAsync();

    public async Task<List<VipApplication>> GetPendingApplicationsAsync()
        => await _unitOfWork.VipApplications.GetByStatusAsync("Pending");

    public async Task<(bool Success, string Error)> ApproveAsync(string applicationId)
    {
        var application = await _unitOfWork.VipApplications.GetByIdAsync(applicationId);
        if (application == null) return (false, "Application not found.");
        if (application.Status != "Pending") return (false, "Application is not pending.");

        await _unitOfWork.VipApplications.UpdateStatusAsync(applicationId, "Approved");

        // Upgrade user role to "VIP Client"
        var roles = await _roleService.GetAllAsync(1, 100);
        var vipRole = roles.FirstOrDefault(r => r.RoleName == "VIP Client");
        if (vipRole != null)
            await _userService.UpdateRoleAsync(application.ClientId, vipRole.Id);

        // Notify applicant
        await _notificationService.CreateNotificationAsync(
            application.ClientId,
            "VIP Application Approved! 🎉",
            "Congratulations! Your VIP application has been approved. You now have VIP Client access.",
            "Info");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> RejectAsync(string applicationId)
    {
        var application = await _unitOfWork.VipApplications.GetByIdAsync(applicationId);
        if (application == null) return (false, "Application not found.");
        if (application.Status != "Pending") return (false, "Application is not pending.");

        await _unitOfWork.VipApplications.UpdateStatusAsync(applicationId, "Rejected");

        // Notify applicant
        await _notificationService.CreateNotificationAsync(
            application.ClientId,
            "VIP Application Rejected",
            "Your VIP application was not approved at this time. Please contact support for details.",
            "Warning");

        return (true, string.Empty);
    }
}
