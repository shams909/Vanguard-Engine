using System.Security.Claims;

namespace Vanguard_Engine.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        return claim?.Value ?? string.Empty;
    }
}
