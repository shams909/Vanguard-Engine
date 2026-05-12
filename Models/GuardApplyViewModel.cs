using System.ComponentModel.DataAnnotations;

namespace Vanguard_Engine.Models;

public class GuardApplyViewModel
{
    [Required(ErrorMessage = "Experience is required")]
    [Display(Name = "Your Experience")]
    public string Experience { get; set; } = string.Empty;

    [Required(ErrorMessage = "Skills are required")]
    [Display(Name = "Your Skills")]
    public string Skills { get; set; } = string.Empty;
}
