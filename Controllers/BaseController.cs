using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Vanguard_Engine.Controllers;

/// <summary>
/// MODULE 3: Base controller providing common helpers for all controllers.
/// Eliminates duplicated GetUserId(), GetUserRole(), and TempData patterns.
/// </summary>
public abstract class BaseController : Controller
{
    // -- Identity Helpers ------------------------------------------------------

    /// <summary>Returns the authenticated user ID from claims. Empty string if not authenticated.</summary>
    protected string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    /// <summary>Returns the authenticated user role name. Empty string if no role claim.</summary>
    protected string GetUserRole() =>
        User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    /// <summary>Returns the authenticated user display name.</summary>
    protected string GetUserName() =>
        User.Identity?.Name ?? "Unknown";

    /// <summary>True if the current user is in the given role (case-insensitive).</summary>
    protected bool IsInRole(string role) =>
        User.IsInRole(role);

    // -- TempData Helpers ------------------------------------------------------

    /// <summary>Sets a success flash message shown in the next view render.</summary>
    protected void SetSuccess(string message) => TempData["Success"] = message;

    /// <summary>Sets an error flash message shown in the next view render.</summary>
    protected void SetError(string message) => TempData["Error"] = message;

    /// <summary>
    /// Sets TempData based on a service result tuple and returns appropriate redirect.
    /// Pattern: TempData[result.Success ? "Success" : "Error"] = result.Success ? successMsg : result.Error
    /// </summary>
    protected void SetResult((bool Success, string Error) result, string successMessage)
    {
        if (result.Success)
            SetSuccess(successMessage);
        else
            SetError(result.Error);
    }

    // -- Ownership Guard -------------------------------------------------------

    /// <summary>
    /// Returns AccessDenied redirect if the given ownerId does not match the current user.
    /// Call this before any resource mutation to enforce ownership.
    /// </summary>
    protected IActionResult? EnforceOwnership(string ownerId)
    {
        if (ownerId != GetUserId())
            return RedirectToAction("AccessDenied", "Auth");
        return null;
    }
}
