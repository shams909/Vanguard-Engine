using System.ComponentModel.DataAnnotations;

namespace Vanguard_Engine.Models;

public class UpdatePhoneViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Phone]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;
}
