using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Models;

public class LoginResult
{
    public bool Success { get; set; }
    public bool IsEmailUnverified { get; set; }
    public User? User { get; set; }
    public string? ErrorMessage { get; set; }
}
