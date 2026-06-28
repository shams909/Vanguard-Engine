using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class RatingService : IRatingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public RatingService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<List<Rating>> GetGuardRatingsAsync(string guardId)
        => await _unitOfWork.Ratings.GetByGuardIdAsync(guardId);

    public async Task<List<Rating>> GetClientRatingsAsync(string clientId)
        => await _unitOfWork.Ratings.GetByClientIdAsync(clientId);

    public async Task<List<Rating>> GetAllRatingsAsync()
        => await _unitOfWork.Ratings.GetPagedAsync(1, 100);

    public async Task<double> GetGuardAverageScoreAsync(string guardId)
    {
        var ratings = await GetGuardRatingsAsync(guardId);
        return ratings.Count == 0 ? 0 : ratings.Average(r => r.Score);
    }

    /// <summary>
    /// MODULE 7: Returns the de-duplicated list of guards who have been assigned to
    /// any of this client's completed ClientRequests. This prevents clients from rating
    /// guards they never actually worked with.
    /// </summary>
    public async Task<List<User>> GetEligibleGuardsToRateAsync(string clientId)
    {
        // Get all completed requests for this client
        var requests = await _unitOfWork.ClientRequests.GetByClientIdAsync(clientId);
        var completedRequests = requests
            .Where(r => r.Status == "Completed" || r.Status == "Active")
            .ToList();

        if (!completedRequests.Any()) return new List<User>();

        // Collect all unique guard IDs across their completed deployments
        var assignedGuardIds = completedRequests
            .Where(r => r.AssignedGuardIds != null)
            .SelectMany(r => r.AssignedGuardIds)
            .Distinct()
            .ToList();

        if (!assignedGuardIds.Any()) return new List<User>();

        // Resolve to User entities
        var eligibleGuards = new List<User>();
        foreach (var guardId in assignedGuardIds)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(guardId);
            if (user != null) eligibleGuards.Add(user);
        }

        return eligibleGuards;
    }

    /// <summary>
    /// MODULE 7: Checks if the client already rated this specific guard.
    /// One rating per (client, guard) pair is enforced.
    /// </summary>
    public async Task<bool> HasAlreadyRatedGuardAsync(string clientId, string guardId)
    {
        var clientRatings = await _unitOfWork.Ratings.GetByClientIdAsync(clientId);
        return clientRatings.Any(r => r.GuardId == guardId);
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public async Task<(bool Success, string Error)> SubmitRatingAsync(
        string clientId, string guardId, string guardName,
        string shiftId, int score, string comments)
    {
        if (score < 1 || score > 5)
            return (false, "Score must be between 1 and 5.");

        if (string.IsNullOrWhiteSpace(guardId) || string.IsNullOrWhiteSpace(clientId))
            return (false, "Invalid user references.");

        // MODULE 7: Relationship validation — client must have worked with this guard
        var eligible = await GetEligibleGuardsToRateAsync(clientId);
        if (!eligible.Any(g => g.Id == guardId))
            return (false, "You can only rate guards who were assigned to your completed deployments.");

        // MODULE 7: Duplicate check — one rating per (client, guard) pair
        if (await HasAlreadyRatedGuardAsync(clientId, guardId))
            return (false, $"You have already submitted a rating for {guardName}. Only one rating per guard is allowed.");

        var rating = new Rating
        {
            ClientId  = clientId,
            GuardId   = guardId,
            GuardName = guardName,
            ShiftId   = shiftId ?? string.Empty,
            Score     = score,
            Comments  = comments ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Ratings.AddAsync(rating);

        // Notify the guard of their new rating
        string stars = new string('\u2605', score) + new string('\u2606', 5 - score);
        await _notificationService.CreateNotificationAsync(
            guardId,
            "You Received a New Rating",
            $"A client rated your service {score}/5 ({stars}). Comments: \"{comments}\"",
            "Info");

        // Notify Admin with the performance summary
        await _notificationService.NotifyRoleAsync(
            "Admin",
            "Guard Performance Rating Submitted",
            $"A client rated {guardName} {score}/5 stars.",
            "Info");

        return (true, string.Empty);
    }
}
