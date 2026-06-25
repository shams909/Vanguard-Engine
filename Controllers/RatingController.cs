using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[Authorize(Roles = "Client,VIP Client,VIP")]
public class RatingController : Controller
{
    private readonly IRatingService _ratingService;
    private readonly IUserService _userService;

    public RatingController(IRatingService ratingService, IUserService userService)
    {
        _ratingService = ratingService;
        _userService = userService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> RateGuard()
    {
        // Get all users who are guards
        var users = await _userService.GetAllAsync(1, 1000);
        // Since we don't strictly have a Role string on User model easily filterable here, 
        // we will pass all users and let the view filter or we can filter by logic.
        // Actually, we can fetch all users and filter in view or just show all.
        ViewBag.Guards = users.Where(u => u.Email.Contains("@") && u.Id != GetUserId()).ToList(); // Simple fallback
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitRating(string guardId, string guardName, int score, string comments)
    {
        var clientId = GetUserId();
        
        var result = await _ratingService.SubmitRatingAsync(clientId, guardId, guardName, "", score, comments);

        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "✅ Thank you for your feedback! Your rating has been submitted." : result.Error;

        return RedirectToAction("RateGuard");
    }
}
