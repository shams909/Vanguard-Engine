using System.ComponentModel.DataAnnotations;
using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Models;

public class UsersCreateViewModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, MinimumLength = 2)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? RoleId { get; set; }

    // Populated from DB for the role dropdown
    public List<Role> Roles { get; set; } = new();
}
