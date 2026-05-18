using System.ComponentModel.DataAnnotations;

namespace Vanguard_Engine.Models;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;
}
