using Newtonsoft.Json;

namespace Vanguard_Engine.Entities;

public class GuardApplication
{
    [JsonProperty("$id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonProperty("jobId")]
    public string? JobId { get; set; }

    [JsonProperty("fullName")]
    public string FullName { get; set; } = string.Empty;

    [JsonProperty("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonProperty("nationalId")]
    public string NationalId { get; set; } = string.Empty;

    [JsonProperty("address")]
    public string Address { get; set; } = string.Empty;

    [JsonProperty("yearsOfExperience")]
    public int YearsOfExperience { get; set; }

    [JsonProperty("experience")]
    public string Experience { get; set; } = string.Empty;

    [JsonProperty("skills")]
    public string Skills { get; set; } = string.Empty;

    [JsonProperty("preferredLocation")]
    public string PreferredLocation { get; set; } = string.Empty;

    [JsonProperty("armedLicense")]
    public bool ArmedLicense { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = "Pending";

    [JsonProperty("$createdAt")]
    public DateTime CreatedAt { get; set; }
}
