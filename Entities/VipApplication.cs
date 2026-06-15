using Newtonsoft.Json;

namespace Vanguard_Engine.Entities;

public class VipApplication
{
    [JsonProperty("$id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("clientId")]
    public string ClientId { get; set; } = string.Empty;

    [JsonProperty("clientName")]
    public string ClientName { get; set; } = string.Empty;

    [JsonProperty("companyName")]
    public string CompanyName { get; set; } = string.Empty;

    [JsonProperty("verificationDetails")]
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Verification details are required.")]
    public string VerificationDetails { get; set; } = string.Empty;

    /// <summary>Pending | Approved | Rejected</summary>
    [JsonProperty("status")]
    public string Status { get; set; } = "Pending";

    [JsonProperty("$createdAt")]
    public DateTime CreatedAt { get; set; }
}
