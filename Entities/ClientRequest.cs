using Newtonsoft.Json;

namespace Vanguard_Engine.Entities;

public class ClientRequest
{
    [JsonProperty("$id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("clientId")]
    public string ClientId { get; set; } = string.Empty;

    [JsonProperty("numberOfGuards")]
    public int NumberOfGuards { get; set; }

    [JsonProperty("location")]
    public string Location { get; set; } = string.Empty;

    [JsonProperty("duration")]
    public string Duration { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = "Pending";

    [JsonProperty("assignedGuardIds")]
    public List<string> AssignedGuardIds { get; set; } = new();

    // ── Lifecycle metadata ────────────────────────────────────────────────────

    /// <summary>Admin-side notes visible only to the client (e.g. "Your request is under review").</summary>
    [JsonProperty("adminNotes")]
    public string? AdminNotes { get; set; }

    /// <summary>Reason provided when request is cancelled or rejected.</summary>
    [JsonProperty("cancelReason")]
    public string? CancelReason { get; set; }

    /// <summary>The confirmed start date/time when the request transitions to Scheduled.</summary>
    [JsonProperty("scheduledDate")]
    public DateTime? ScheduledDate { get; set; }

    /// <summary>UTC timestamp when the request was completed — used for mission history.</summary>
    [JsonProperty("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [JsonProperty("$createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("$updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
