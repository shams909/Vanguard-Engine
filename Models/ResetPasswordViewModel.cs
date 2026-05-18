using System.ComponentModel.DataAnnotations;

namespace Vanguard_Engine.Models;

public class ResetPasswordViewModel
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Otp { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
