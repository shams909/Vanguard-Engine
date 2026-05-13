using System.ComponentModel.DataAnnotations;
using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Entities;

public class HiringNotice
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string ReferenceCode { get; set; } = string.Empty; // e.g. V-REQ-1001

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Requirements { get; set; } = string.Empty;

    [Required]
    public string Location { get; set; } = string.Empty;

    [Required]
    public string JobType { get; set; } = "Full-time"; // Full-time, Contract, Urgent

    [Required]
    public string Priority { get; set; } = "Normal"; // High, Normal, Low

    public string? SalaryRange { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiryDate { get; set; }

    public string Status { get; set; } = "Open"; // Open, Closed

    public string PostedByUserId { get; set; } = string.Empty;
}
