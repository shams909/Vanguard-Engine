using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Services;

public interface IRatingService
{
    Task<(bool Success, string Error)> SubmitRatingAsync(string clientId, string guardId, string guardName, string shiftId, int score, string comments);
    Task<List<Rating>> GetGuardRatingsAsync(string guardId);
    Task<double> GetGuardAverageScoreAsync(string guardId);
    Task<List<Rating>> GetAllRatingsAsync();
}
