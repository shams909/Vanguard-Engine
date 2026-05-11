using Newtonsoft.Json;

namespace Vanguard_Engine.Entities;

public class GuardApplication
{
    [JsonProperty("$id")]
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Experience { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
}
