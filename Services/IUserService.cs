using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IUserService
{
    Task<List<User>> GetAllAsync(int pageNumber, int pageSize);
    Task<User?> GetByIdAsync(string id);
    Task<User> CreateAsync(string username, string email, string password, string? address, string? roleId);
    Task<bool> UpdateAsync(string id, string username, string email, string? password, string? address, string? roleId);
    Task<bool> UpdateRoleAsync(string userId, string newRoleId);
    Task<bool> DeleteAsync(string id);

    // ── Guard Lifecycle Management ────────────────────────────────────────────
    /// <summary>Suspends a guard (disciplinary). Sets GuardStatus = "Suspended".</summary>
    Task<bool> SuspendGuardAsync(string userId);

    /// <summary>Reinstates a suspended or unavailable guard back to Available.</summary>
    Task<bool> ReinstateGuardAsync(string userId);

    /// <summary>Marks a guard as self-reported unavailable (sick leave, etc.).</summary>
    Task<bool> SetUnavailableAsync(string userId);

    /// <summary>Returns all users with a specific operational guard status.</summary>
    Task<List<User>> GetGuardsByStatusAsync(string status);
}

