using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Vanguard_Engine.Entities;

public class HiringNotice
{
    [JsonProperty("$id")]
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [JsonProperty("referenceCode")]
    public string ReferenceCode { get; set; } = string.Empty; // e.g. V-REQ-1001

    [Required]
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [JsonProperty("requirements")]
    public string Requirements { get; set; } = string.Empty;

    [Required]
    [JsonProperty("location")]
    public string Location { get; set; } = string.Empty;

    [Required]
    [JsonProperty("jobType")]
    public string JobType { get; set; } = "Full-time"; // Full-time, Contract, Urgent

    [Required]
    [JsonProperty("priority")]
    public string Priority { get; set; } = "Normal"; // High, Normal, Low

    [JsonProperty("salaryRange")]
    public string? SalaryRange { get; set; }

    [JsonProperty("$createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("expiryDate")]
    public DateTime? ExpiryDate { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = "Open"; // Open, Closed, Filled

    [JsonProperty("postedByUserId")]
    public string PostedByUserId { get; set; } = string.Empty;

    // ── MODULE 6: Capacity Tracking ───────────────────────────────────────────

    /// <summary>Total guard positions available in this notice.</summary>
    [JsonProperty("numberOfPositions")]
    public int NumberOfPositions { get; set; } = 1;

    /// <summary>How many positions have been filled (accepted applications).</summary>
    [JsonProperty("filledPositions")]
    public int FilledPositions { get; set; } = 0;

    /// <summary>True when NumberOfPositions has been fully filled.</summary>
    public bool IsFull => FilledPositions >= NumberOfPositions;
}

