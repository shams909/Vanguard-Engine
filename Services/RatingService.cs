using Vanguard_Engine.Entities;
using Vanguard_Engine.UnitOfWork;

namespace Vanguard_Engine.Services;

public class RatingService : IRatingService
{
    private readonly IUnitOfWork _unitOfWork;

    public RatingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool Success, string Error)> SubmitRatingAsync(string clientId, string guardId, string guardName, string shiftId, int score, string comments)
    {
        if (score < 1 || score > 5) return (false, "Score must be between 1 and 5.");
        if (string.IsNullOrWhiteSpace(guardId) || string.IsNullOrWhiteSpace(clientId)) return (false, "Invalid user references.");

        // In a real scenario, we might want to check if they already rated this exact shift
        
        var rating = new Rating
        {
            ClientId = clientId,
            GuardId = guardId,
            GuardName = guardName,
            ShiftId = shiftId ?? "",
            Score = score,
            Comments = comments ?? "",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Ratings.AddAsync(rating);
        return (true, string.Empty);
    }

    public async Task<List<Rating>> GetGuardRatingsAsync(string guardId)
    {
        return await _unitOfWork.Ratings.GetByGuardIdAsync(guardId);
    }

    public async Task<double> GetGuardAverageScoreAsync(string guardId)
    {
        var ratings = await GetGuardRatingsAsync(guardId);
        if (!ratings.Any()) return 0;
        return ratings.Average(r => r.Score);
    }

    public async Task<List<Rating>> GetAllRatingsAsync()
    {
        var ratings = await _unitOfWork.Ratings.GetPagedAsync(1, 100);
        return ratings;
    }
}
