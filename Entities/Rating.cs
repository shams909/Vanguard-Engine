using Newtonsoft.Json;

namespace Vanguard_Engine.Entities;

public class Rating
{
    [JsonProperty("$id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("clientId")]
    public string ClientId { get; set; } = string.Empty;

    [JsonProperty("guardId")]
    public string GuardId { get; set; } = string.Empty;

    [JsonProperty("guardName")]
    public string GuardName { get; set; } = string.Empty;

    [JsonProperty("score")]
    public int Score { get; set; }

    [JsonProperty("comments")]
    public string Comments { get; set; } = string.Empty;

    [JsonProperty("shiftId")]
    public string ShiftId { get; set; } = string.Empty;

    [JsonProperty("$createdAt")]
    public DateTime CreatedAt { get; set; }
}
