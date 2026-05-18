using System.ComponentModel.DataAnnotations;

namespace Vanguard_Engine.Models;

public class VerifyOtpViewModel
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the 6-digit OTP code.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must contain only numbers.")]
    [Display(Name = "OTP Code")]
    public string Otp { get; set; } = string.Empty;
}
