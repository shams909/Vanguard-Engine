using System.ComponentModel.DataAnnotations;

namespace Vanguard_Engine.Models;

public class VIPRequestViewModel
{
    [Required(ErrorMessage = "Please select a protection type.")]
    [Display(Name = "Protection Type")]
    public string ProtectionType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please specify whether armed service is required.")]
    [Display(Name = "Armed Service Required")]
    public bool ArmedRequired { get; set; }

    [Required(ErrorMessage = "Please specify the number of elite guards needed.")]
    [Range(1, 20, ErrorMessage = "You must request between 1 and 20 elite protection officers.")]
    [Display(Name = "Number of Elite Officers")]
    public int NumberOfGuards { get; set; } = 1;

    [Required(ErrorMessage = "Please select a service duration.")]
    [Display(Name = "Service Duration")]
    public string Duration { get; set; } = string.Empty;
}
