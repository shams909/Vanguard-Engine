using Microsoft.AspNetCore.Mvc.RazorPages;
using Vanguard_Engine.DTOs.Users;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Pages.Users;

public class IndexModel : PageModel
{
    private readonly IUserService _userService;

    public IndexModel(IUserService userService)
    {
        _userService = userService;
    }

    public List<UserDto> Users { get; private set; } = new();
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }

    public async Task OnGetAsync(int pageNumber = 1, int pageSize = 10)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        Users = await _userService.GetAllAsync(pageNumber, pageSize);
    }
}
