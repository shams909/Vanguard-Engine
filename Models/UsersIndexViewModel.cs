using Vanguard_Engine.Entities;

namespace Vanguard_Engine.Models;

public class UsersIndexViewModel
{
    public List<User> Users { get; set; } = new();
    public List<Role> Roles { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
