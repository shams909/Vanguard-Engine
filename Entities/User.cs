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
    
    [JsonProperty("roleId")]
    public string? RoleId { get; set; }
    
    [JsonProperty("lastLogin")]
    public DateTime LastLogin { get; set; }
    
    [JsonIgnore]
    public Role? Role { get; set; }
}
