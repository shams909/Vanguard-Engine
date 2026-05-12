using Vanguard_Engine.DTOs.Roles;

namespace Vanguard_Engine.Models;

public class RolesIndexViewModel
{
    public List<RoleDto> Roles { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
