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

    /// <summary>Scheduled | Cancelled | Completed</summary>
    [JsonProperty("status")]
    public string Status { get; set; } = "Scheduled";

    [JsonProperty("$createdAt")]
    public DateTime CreatedAt { get; set; }
}
