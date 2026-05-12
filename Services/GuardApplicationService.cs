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

    public async Task<bool> ApplyAsync(string userId, string experience, string skills)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(experience) || string.IsNullOrWhiteSpace(skills))
            return false;

        // Prevent duplicate active application
        var existing = await _unitOfWork.GuardApplications.GetByUserIdAsync(userId);
        if (existing != null && (existing.Status == "Pending" || existing.Status == "Approved"))
            return false;

        var application = new GuardApplication
        {
            UserId = userId,
            Experience = experience,
            Skills = skills,
            Status = "Pending"
        };

        await _unitOfWork.GuardApplications.AddAsync(application);
        return true;
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

    public async Task<bool> ApproveAsync(string id)
    {
        var application = await _unitOfWork.GuardApplications.GetByIdAsync(id);
        if (application == null || application.Status != "Pending") return false;

        await _unitOfWork.GuardApplications.UpdateStatusAsync(id, "Approved");
        return true;
    }

    public async Task<bool> RejectAsync(string id)
    {
        var application = await _unitOfWork.GuardApplications.GetByIdAsync(id);
        if (application == null || application.Status != "Pending") return false;

        await _unitOfWork.GuardApplications.UpdateStatusAsync(id, "Rejected");
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var application = await _unitOfWork.GuardApplications.GetByIdAsync(id);
        if (application == null || application.Status != "Pending") return false;

        await _unitOfWork.GuardApplications.DeleteAsync(id);
        return true;
    }
}
