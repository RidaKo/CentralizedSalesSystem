using CentralizedSalesSystem.API.Models.Business;
using CentralizedSalesSystem.API.Models.Business.DTOs;
using CentralizedSalesSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralizedSalesSystem.API.Controllers;

[Route("businesses")]
[Authorize]
[ApiController]
public class BusinessesController : ControllerBase
{
    private readonly IBusinessService _service;

    public BusinessesController(IBusinessService service)
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
        [FromQuery] string? filterByAddress = null,
        [FromQuery] string? filterByEmail = null,
        [FromQuery] Currency? filterByCurrency = null,
        [FromQuery] SubscriptionPlan? filterBySubscriptionPlan = null,
        [FromQuery] long? filterByOwnerId = null)
    {
        if (page < 1) page = 1;
        if (limit < 1) limit = 20;

        var result = await _service.GetAllAsync(page, limit, sortBy, sortDirection,
            filterByName, filterByPhone, filterByAddress, filterByEmail,
            filterByCurrency, filterBySubscriptionPlan, filterByOwnerId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BusinessCreateDto dto)
    {
        if (dto == null) return BadRequest();
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { businessId = created.Id }, created);
    }

    [HttpGet("{businessId:long}")]
    public async Task<IActionResult> GetById([FromRoute] long businessId)
    {
        var b = await _service.GetByIdAsync(businessId);
        if (b == null) return NotFound();
        return Ok(b);
    }

    [HttpPatch("{businessId:long}")]
    public async Task<IActionResult> Patch([FromRoute] long businessId, [FromBody] BusinessPatchDto dto)
    {
        if (dto == null) return BadRequest();
        var updated = await _service.PatchAsync(businessId, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{businessId:long}")]
    public async Task<IActionResult> Delete([FromRoute] long businessId)
    {
        var ok = await _service.DeleteAsync(businessId);
        if (!ok) return NotFound();
        return Ok();
    }
}
