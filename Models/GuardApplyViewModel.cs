using System.ComponentModel.DataAnnotations;

namespace Vanguard_Engine.Models;

public class GuardApplyViewModel
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 150 characters")]
    [Display(Name = "Full Legal Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Enter a valid phone number")]
    [Display(Name = "Phone Number")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "National ID is required")]
    [StringLength(30, MinimumLength = 5, ErrorMessage = "Enter a valid National ID / NID")]
    [Display(Name = "National ID (NID)")]
    public string NationalId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Please provide a complete address")]
    [Display(Name = "Current Address")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Years of experience is required")]
    [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
    [Display(Name = "Years of Security Experience")]
    public int YearsOfExperience { get; set; }

    [Required(ErrorMessage = "Experience description is required")]
    [StringLength(5000, MinimumLength = 50, ErrorMessage = "Please describe your experience in at least 50 characters")]
    [Display(Name = "Work Experience Details")]
    public string Experience { get; set; } = string.Empty;

    [Required(ErrorMessage = "Skills are required")]
    [StringLength(5000, MinimumLength = 20, ErrorMessage = "Please list your skills (at least 20 characters)")]
    [Display(Name = "Skills & Certifications")]
    public string Skills { get; set; } = string.Empty;

    [Required(ErrorMessage = "Preferred location is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Please specify your preferred deployment location")]
    [Display(Name = "Preferred Deployment Location")]
    public string PreferredLocation { get; set; } = string.Empty;

    [Display(Name = "Armed Guard License")]
    public bool ArmedLicense { get; set; }
}
