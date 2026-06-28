using Newtonsoft.Json;

namespace Vanguard_Engine.Entities;

/// <summary>
/// MODULE 11: Immutable audit record written whenever a state transition occurs
/// on any major entity (ClientRequest, VIPRequest, GuardApplication, User, etc.).
/// </summary>
public class AuditLog
{
    [JsonProperty("$id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>The entity type being audited (e.g. "ClientRequest", "VIPRequest", "User").</summary>
    [JsonProperty("entityType")]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>The Appwrite document ID of the entity that changed.</summary>
    [JsonProperty("entityId")]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>The action performed (e.g. "StatusChanged", "GuardAssigned", "Cancelled").</summary>
    [JsonProperty("action")]
    public string Action { get; set; } = string.Empty;

    /// <summary>The status/state before the change.</summary>
    [JsonProperty("fromValue")]
    public string? FromValue { get; set; }

    /// <summary>The status/state after the change.</summary>
    [JsonProperty("toValue")]
    public string? ToValue { get; set; }

    /// <summary>Optional human-readable note about why the change was made.</summary>
    [JsonProperty("notes")]
    public string? Notes { get; set; }

    /// <summary>The user ID who performed the action.</summary>
    [JsonProperty("performedByUserId")]
    public string PerformedByUserId { get; set; } = string.Empty;

    /// <summary>The role of the user who performed the action (Admin, Client, etc.).</summary>
    [JsonProperty("performedByRole")]
    public string? PerformedByRole { get; set; }

    [JsonProperty("$createdAt")]
    public DateTime CreatedAt { get; set; }
}
