using Vanguard_Engine.DTOs.Roles;

namespace Vanguard_Engine.Models;

public class RolesCreateViewModel
{
    public CreateRoleDto Role { get; set; } = new(string.Empty, null);
}
