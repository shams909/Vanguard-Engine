using Newtonsoft.Json;

namespace Vanguard_Engine.Entities;

public class VIPRequest
{
    [JsonProperty("$id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("vipClientId")]
    public string VipClientId { get; set; } = string.Empty;

    [JsonProperty("protectionType")]
    public string ProtectionType { get; set; } = string.Empty;

    [JsonProperty("armedRequired")]
    public bool ArmedRequired { get; set; }

    [JsonProperty("numberOfGuards")]
    public int NumberOfGuards { get; set; }

    [JsonProperty("duration")]
    public string Duration { get; set; } = string.Empty;

    /// <summary>Location / venue where protection is required.</summary>
    [JsonProperty("location")]
    public string? Location { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = "Pending";

    [JsonProperty("assignedGuardIds")]
    public List<string> AssignedGuardIds { get; set; } = new();

    // ── Lifecycle Metadata ────────────────────────────────────────────────────

    /// <summary>Reason shown to client when the request is rejected.</summary>
    [JsonProperty("rejectionReason")]
    public string? RejectionReason { get; set; }

    /// <summary>Reason shown to client when the request is cancelled.</summary>
    [JsonProperty("cancelReason")]
    public string? CancelReason { get; set; }

    /// <summary>Confirmed protection start date/time (set on Scheduled transition).</summary>
    [JsonProperty("scheduledAt")]
    public DateTime? ScheduledAt { get; set; }

    /// <summary>UTC timestamp when the mission was marked Completed.</summary>
    [JsonProperty("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [JsonProperty("$createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("$updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

