using CentralizedSalesSystem.API.Models.Auth.enums;
using CentralizedSalesSystem.API.Models.Users;
using CentralizedSalesSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralizedSalesSystem.API.Controllers;

[Route("users")]
[Authorize]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc",
        [FromQuery] string? filterByName = null,
        [FromQuery] string? filterByPhone = null,
        [FromQuery] string? filterByEmail = null,
        [FromQuery] Status? filterByActivity = null,
        [FromQuery] long? filterByBusinessId = null)
    {
        if (page < 1) page = 1;
        if (limit < 1) limit = 20;

        var result = await _service.GetAllAsync(page, limit, sortBy, sortDirection,
            filterByName, filterByPhone, filterByEmail, filterByActivity, filterByBusinessId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
    {
        if (dto == null) return BadRequest();
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { userId = created.Id }, created);
    }

    [HttpGet("{userId:long}")]
    public async Task<IActionResult> GetById([FromRoute] long userId)
    {
        var user = await _service.GetByIdAsync(userId);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPatch("{userId:long}")]
    public async Task<IActionResult> Patch([FromRoute] long userId, [FromBody] UserPatchDto dto)
    {
        if (dto == null) return BadRequest();
        var updated = await _service.PatchAsync(userId, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{userId:long}")]
    public async Task<IActionResult> Delete([FromRoute] long userId)
    {
        var ok = await _service.DeleteAsync(userId);
        if (!ok) return NotFound();
        return Ok();
    }
}
