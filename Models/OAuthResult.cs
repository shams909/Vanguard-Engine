using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Models;

public class OAuthResult
{
    public bool Success { get; set; }
    public User? User { get; set; }
    public bool IsNewUser { get; set; }
    public string? AppwriteUserId { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? ErrorMessage { get; set; }
}
