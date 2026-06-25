using Newtonsoft.Json;

namespace Vanguard_Engine.Entities;

public class Incident
{
    [JsonProperty("$id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("reportedByUserId")]
    public string ReportedByUserId { get; set; } = string.Empty;

    [JsonProperty("reportedByName")]
    public string ReportedByName { get; set; } = string.Empty;

    /// <summary>Guard | Client</summary>
    [JsonProperty("reporterRole")]
    public string ReporterRole { get; set; } = string.Empty;

    /// <summary>Incident | Complaint</summary>
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Open | Resolved</summary>
    [JsonProperty("status")]
    public string Status { get; set; } = "Open";

    [JsonProperty("resolutionNotes")]
    public string ResolutionNotes { get; set; } = string.Empty;

    [JsonProperty("resolvedByAdminId")]
    public string ResolvedByAdminId { get; set; } = string.Empty;

    [JsonProperty("$createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("$updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
