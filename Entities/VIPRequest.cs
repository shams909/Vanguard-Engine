using Newtonsoft.Json;

namespace Vanguard_Engine.Entities;

public class VIPRequest
{
    [JsonProperty("$id")]
    public string Id { get; set; } = string.Empty;
    public string VipClientId { get; set; } = string.Empty;
    public string ProtectionType { get; set; } = string.Empty;
    public bool ArmedRequired { get; set; }
    public int NumberOfGuards { get; set; }
    public string Duration { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
}
