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
    private readonly INotificationService _notificationService;

    public RatingController(IRatingService ratingService, IUserService userService, INotificationService notificationService)
    {
        _ratingService = ratingService;
        _userService = userService;
        _notificationService = notificationService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> RateGuard()
    {
        var users = await _userService.GetAllAsync(1, 1000);
        ViewBag.Guards = users.Where(u => u.Email.Contains("@") && u.Id != GetUserId()).ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitRating(string guardId, string guardName, int score, string comments)
    {
        var clientId = GetUserId();
        var clientName = User.Identity?.Name ?? "A Client";
        
        var result = await _ratingService.SubmitRatingAsync(clientId, guardId, guardName, "", score, comments);

        if (result.Success)
        {
            TempData["Success"] = "✅ Thank you for your feedback! Your rating has been submitted.";

            // 🔔 Real-time: Notify the rated Guard instantly
            if (!string.IsNullOrEmpty(guardId))
            {
                string stars = new string('★', score) + new string('☆', 5 - score);
                await _notificationService.CreateNotificationAsync(
                    userId: guardId,
                    title: "You Received a New Rating",
                    message: $"{clientName} rated your service {score}/5 ({stars}). Comments: \"{comments}\"",
                    type: "Info"
                );
            }

            // 🔔 Real-time: Notify Admins about the new rating summary
            await _notificationService.NotifyRoleAsync(
                roleName: "Admin",
                title: "Guard Performance Rating Submitted",
                message: $"{clientName} rated {guardName} {score}/5 stars.",
                type: "Info"
            );
        }
        else
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction("RateGuard");
    }
}
