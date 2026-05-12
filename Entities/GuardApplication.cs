using Newtonsoft.Json;

namespace Vanguard_Engine.Entities;

public class GuardApplication
{
    [JsonProperty("$id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonProperty("experience")]
    public string Experience { get; set; } = string.Empty;

    [JsonProperty("skills")]
    public string Skills { get; set; } = string.Empty;

    [JsonProperty("status")]
    public string Status { get; set; } = "Pending";

    [JsonProperty("$createdAt")]
    public DateTime CreatedAt { get; set; }
}
