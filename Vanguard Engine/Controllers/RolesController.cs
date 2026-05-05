using Microsoft.AspNetCore.Mvc;
using Vanguard_Engine.DTOs.Roles;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoleDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var roles = await _roleService.GetAllAsync(pageNumber, pageSize);
        return Ok(roles);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoleDto>> GetById(int id)
    {
        var role = await _roleService.GetByIdAsync(id);
        return role is null ? NotFound() : Ok(role);
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create(CreateRoleDto dto)
    {
        var role = await _roleService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateRoleDto dto)
    {
        var updated = await _roleService.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _roleService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
