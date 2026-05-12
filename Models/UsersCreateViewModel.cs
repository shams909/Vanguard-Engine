using Vanguard_Engine.DTOs.Roles;
using Vanguard_Engine.DTOs.Users;

namespace Vanguard_Engine.Models;

public class UsersCreateViewModel
{
    public CreateUserDto CreateUser { get; set; } = new(string.Empty, string.Empty, string.Empty, null, null, DateTime.UtcNow);
    public List<RoleDto> Roles { get; set; } = new();
}
