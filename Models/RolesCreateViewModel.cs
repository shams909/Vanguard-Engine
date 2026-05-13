using System.ComponentModel.DataAnnotations;

namespace Vanguard_Engine.Models;

public class RolesCreateViewModel
{
    [Required(ErrorMessage = "Role name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string RoleName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}
