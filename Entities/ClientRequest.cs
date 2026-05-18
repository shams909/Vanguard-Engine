using Newtonsoft.Json;

namespace Vanguard_Engine.Entities;

public class ClientRequest
{
    [JsonProperty("$id")]
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public int NumberOfGuards { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";

    [JsonProperty("assignedGuardIds")]
    public List<string> AssignedGuardIds { get; set; } = new();
}
