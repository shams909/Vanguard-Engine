using Newtonsoft.Json;

namespace Vanguard_Engine.Entities;

public class GuardShift
{
    [JsonProperty("$id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("guardId")]
    public string GuardId { get; set; } = string.Empty;

    [JsonProperty("guardName")]
    public string GuardName { get; set; } = string.Empty;

    [JsonProperty("assignedShiftId")]
    public string AssignedShiftId { get; set; } = string.Empty;

    /// <summary>ISO 8601 UTC string — stored as string for Appwrite compatibility.</summary>
    [JsonProperty("checkInTime")]
    public string CheckInTime { get; set; } = string.Empty;

    /// <summary>ISO 8601 UTC string — null until guard checks out.</summary>
    [JsonProperty("checkOutTime")]
    public string? CheckOutTime { get; set; }

    /// <summary>Calculated at checkout (total minutes on shift).</summary>
    [JsonProperty("durationMinutes")]
    public int DurationMinutes { get; set; }

    /// <summary>Active | Completed</summary>
    [JsonProperty("status")]
    public string Status { get; set; } = "Active";

    [JsonProperty("$createdAt")]
    public DateTime CreatedAt { get; set; }

    // ── Computed helpers ───────────────────────────────────────────────────
    public DateTime CheckInDateTime =>
        DateTime.TryParse(CheckInTime, null,
            System.Globalization.DateTimeStyles.RoundtripKind, out var dt)
            ? dt : DateTime.MinValue;

    public DateTime? CheckOutDateTime =>
        !string.IsNullOrEmpty(CheckOutTime) &&
        DateTime.TryParse(CheckOutTime, null,
            System.Globalization.DateTimeStyles.RoundtripKind, out var dt)
            ? dt : null;

    public string DurationDisplay
    {
        get
        {
            if (DurationMinutes <= 0) return "—";
            var h = DurationMinutes / 60;
            var m = DurationMinutes % 60;
            return h > 0 ? $"{h}h {m}m" : $"{m}m";
        }
    }
}
