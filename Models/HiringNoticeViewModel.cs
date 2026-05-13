using System.ComponentModel.DataAnnotations;

namespace Vanguard_Engine.Models;

public class HiringNoticeViewModel
{
    public string? Id { get; set; }

    [Required]
    [Display(Name = "Job Title")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Job Type")]
    public string JobType { get; set; } = "Full-time";

    [Required]
    public string Priority { get; set; } = "Normal";

    [Required]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.MultilineText)]
    public string Requirements { get; set; } = string.Empty;

    [Required]
    public string Location { get; set; } = string.Empty;

    [Display(Name = "Salary Range (Optional)")]
    public string? SalaryRange { get; set; }

    [Display(Name = "Expiry Date (Optional)")]
    [DataType(DataType.Date)]
    public DateTime? ExpiryDate { get; set; }
}
