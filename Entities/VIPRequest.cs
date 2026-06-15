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

    [JsonProperty("status")]
    public string Status { get; set; } = "Pending";

    [JsonProperty("assignedGuardIds")]
    public List<string> AssignedGuardIds { get; set; } = new();

    [JsonProperty("$createdAt")]
    public DateTime CreatedAt { get; set; }
}
