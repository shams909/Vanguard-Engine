using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[Authorize(Roles = "Client,VIP Client")]
public class RatingController : BaseController
{
    private readonly IRatingService _ratingService;

    public RatingController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    // ── MODULE 7: Only guards who served this client appear in the rating form ──

    [HttpGet]
    public async Task<IActionResult> RateGuard()
    {
        var clientId = GetUserId();

        // MODULE 7: Only eligible guards (assigned to client's completed requests)
        var eligibleGuards = await _ratingService.GetEligibleGuardsToRateAsync(clientId);

        // Build a map of already-rated guard IDs so the view can disable those options
        var alreadyRated = new HashSet<string>();
        foreach (var guard in eligibleGuards)
        {
            if (await _ratingService.HasAlreadyRatedGuardAsync(clientId, guard.Id))
                alreadyRated.Add(guard.Id);
        }

        ViewBag.EligibleGuards = eligibleGuards;
        ViewBag.AlreadyRated   = alreadyRated;

        if (!eligibleGuards.Any())
            SetError("You have no completed deployments to rate yet.");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitRating(string guardId, string guardName, int score, string comments)
    {
        var clientId   = GetUserId();
        var clientName = GetUserName();

        var result = await _ratingService.SubmitRatingAsync(
            clientId, guardId, guardName, string.Empty, score, comments);

        if (result.Success)
            SetSuccess("Thank you for your feedback! Your rating has been submitted.");
        else
            SetError(result.Error);

        return RedirectToAction(nameof(RateGuard));
    }

    // ── My Submitted Ratings ──────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> MyRatings()
    {
        var ratings = await _ratingService.GetClientRatingsAsync(GetUserId());
        return View(ratings);
    }
}
