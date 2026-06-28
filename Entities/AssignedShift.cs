using Newtonsoft.Json;

namespace Vanguard_Engine.Entities;

public class AssignedShift
{
    [JsonProperty("$id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("guardId")]
    public string GuardId { get; set; } = string.Empty;

    [JsonProperty("guardName")]
    public string GuardName { get; set; } = string.Empty;

    /// <summary>ISO Date string (YYYY-MM-DD)</summary>
    [JsonProperty("shiftDate")]
    public string ShiftDate { get; set; } = string.Empty;

    /// <summary>HH:mm format</summary>
    [JsonProperty("startTime")]
    public string StartTime { get; set; } = string.Empty;

    /// <summary>HH:mm format</summary>
    [JsonProperty("endTime")]
    public string EndTime { get; set; } = string.Empty;

    /// <summary>Scheduled | Active | Completed | Cancelled</summary>
    [JsonProperty("status")]
    public string Status { get; set; } = "Scheduled";

    // ── MODULE 8: Contextual Fields ───────────────────────────────────────────

    /// <summary>Links this shift to its originating ClientRequest deployment.</summary>
    [JsonProperty("clientRequestId")]
    public string? ClientRequestId { get; set; }

    /// <summary>Physical location/venue of the deployment.</summary>
    [JsonProperty("location")]
    public string? Location { get; set; }

    /// <summary>Supervisor notes or special instructions for the guard.</summary>
    [JsonProperty("notes")]
    public string? Notes { get; set; }

    [JsonProperty("$createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("$updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

