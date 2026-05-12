using Vanguard_Engine.DTOs.Roles;
using Vanguard_Engine.DTOs.Users;

namespace Vanguard_Engine.Models;

public class UsersIndexViewModel
{
    public List<UserDto> Users { get; set; } = new();
    public List<RoleDto> Roles { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
