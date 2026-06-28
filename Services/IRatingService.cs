using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IRatingService
{
    // ── Queries ───────────────────────────────────────────────────────────────
    Task<List<Rating>> GetGuardRatingsAsync(string guardId);
    Task<List<Rating>> GetClientRatingsAsync(string clientId);
    Task<List<Rating>> GetAllRatingsAsync();
    Task<double> GetGuardAverageScoreAsync(string guardId);

    /// <summary>
    /// MODULE 7: Returns only guards who have been assigned to the client's requests.
    /// Prevents clients from rating guards they never worked with.
    /// </summary>
    Task<List<User>> GetEligibleGuardsToRateAsync(string clientId);

    /// <summary>
    /// Returns true if this client has already submitted a rating for this guard.
    /// Used to prevent duplicate ratings per guard per client.
    /// </summary>
    Task<bool> HasAlreadyRatedGuardAsync(string clientId, string guardId);

    // ── Commands ──────────────────────────────────────────────────────────────
    Task<(bool Success, string Error)> SubmitRatingAsync(
        string clientId, string guardId, string guardName,
        string shiftId, int score, string comments);
}

