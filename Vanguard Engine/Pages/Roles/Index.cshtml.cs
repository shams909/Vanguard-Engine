using Microsoft.AspNetCore.Mvc.RazorPages;
using Vanguard_Engine.DTOs.Roles;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Pages.Roles;

public class IndexModel : PageModel
{
    private readonly IRoleService _roleService;

    public IndexModel(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public List<RoleDto> Roles { get; private set; } = new();
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }

    public async Task OnGetAsync(int pageNumber = 1, int pageSize = 10)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        Roles = await _roleService.GetAllAsync(pageNumber, pageSize);
    }
}
