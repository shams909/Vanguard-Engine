namespace Vanguard_Engine.Entities;

using Newtonsoft.Json;

public class User
{
    [JsonProperty("$id")]
    public string Id { get; set; } = string.Empty;
    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonProperty("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;
    
    [JsonProperty("address")]
    public string? Address { get; set; }
    
    [JsonProperty("phoneNumber")]
    public string? PhoneNumber { get; set; }
    
    [JsonProperty("roleId")]
    public string? RoleId { get; set; }
    
    [JsonProperty("lastLogin")]
    public DateTime LastLogin { get; set; }

    [JsonProperty("isEmailVerified")]
    public bool IsEmailVerified { get; set; }

    [JsonProperty("verificationToken")]
    public string? VerificationToken { get; set; }

    [JsonProperty("verificationTokenExpiry")]
    public DateTime? VerificationTokenExpiry { get; set; }
    
    [JsonProperty("resetToken")]
    public string? ResetToken { get; set; }

    [JsonProperty("resetTokenExpiry")]
    public DateTime? ResetTokenExpiry { get; set; }

    /// <summary>
    /// Operational availability status for guards only.
    /// Values: Available | Assigned | OnDuty | Unavailable | Suspended
    /// Null for non-guard users.
    /// </summary>
    [JsonProperty("guardStatus")]
    public string? GuardStatus { get; set; }

    [JsonIgnore]
    public Role? Role { get; set; }
}
