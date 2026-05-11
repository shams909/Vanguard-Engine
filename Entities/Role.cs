namespace Vanguard_Engine.Entities;

using Newtonsoft.Json;

public class Role
{
    [JsonProperty("$id")]
    public string Id { get; set; } = string.Empty;
    [JsonProperty("roleName")]
    public string RoleName { get; set; } = string.Empty;
    
    [JsonProperty("description")]
    public string? Description { get; set; }
    
    [JsonIgnore]
    public ICollection<User> Users { get; set; } = new List<User>();
}
