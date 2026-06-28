using System.ComponentModel.DataAnnotations;

namespace Vanguard_Engine.Models;

public class ClientRequestViewModel
{
    [Required(ErrorMessage = "Please specify the number of guards needed.")]
    [Range(1, 100, ErrorMessage = "You must request between 1 and 100 guards.")]
    [Display(Name = "Number of Guards")]
    public int NumberOfGuards { get; set; }

    [Required(ErrorMessage = "Please specify the deployment location.")]
    [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
    [Display(Name = "Patrol Location")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please specify the shift duration.")]
    [StringLength(100, ErrorMessage = "Duration detail cannot exceed 100 characters.")]
    [Display(Name = "Deployment Duration")]
    public string Duration { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    [Display(Name = "Additional Details")]
    public string? Description { get; set; }
}

