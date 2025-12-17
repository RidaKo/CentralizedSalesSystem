using CentralizedSalesSystem.API.Authorization;
using CentralizedSalesSystem.API.Models.Auth.enums;
using CentralizedSalesSystem.API.Models.Business;
using CentralizedSalesSystem.API.Models.Business.DTOs;
using CentralizedSalesSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    [AuthorizePermission(nameof(PermissionCode.BUSINESS_VIEW))]
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
    [AuthorizePermission(nameof(PermissionCode.BUSINESS_UPDATE))]
    public async Task<IActionResult> Create([FromBody] BusinessCreateDto dto)
    {
        if (dto == null) return BadRequest();
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { businessId = created.Id }, created);
    }

    [HttpGet("{businessId:long}")]
    public async Task<IActionResult> GetById([FromRoute] long businessId)
    {
        // Allow if the caller has BUSINESS_VIEW/MANAGE_ALL, or if they belong to the same business.
        var user = HttpContext.User;
        var hasPerm = HasPermission(user, PermissionCode.BUSINESS_VIEW)
                      || HasPermission(user, PermissionCode.MANAGE_ALL);
        var isSameBusiness = user.FindFirst("businessId")?.Value == businessId.ToString();
        if (!hasPerm && !isSameBusiness)
        {
            return Forbid();
        }

        var b = await _service.GetByIdAsync(businessId);
        if (b == null) return NotFound();
        return Ok(b);
    }

    [HttpPatch("{businessId:long}")]
    [AuthorizePermission(nameof(PermissionCode.BUSINESS_UPDATE))]
    public async Task<IActionResult> Patch([FromRoute] long businessId, [FromBody] BusinessPatchDto dto)
    {
        if (dto == null) return BadRequest();
        var updated = await _service.PatchAsync(businessId, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{businessId:long}")]
    [AuthorizePermission(nameof(PermissionCode.BUSINESS_DELETE))]
    public async Task<IActionResult> Delete([FromRoute] long businessId)
    {
        var ok = await _service.DeleteAsync(businessId);
        if (!ok) return NotFound();
        return Ok();
    }

    private static bool HasPermission(ClaimsPrincipal user, PermissionCode code) =>
        user.Claims.Any(c =>
            (c.Type == PermissionAuthorizationHandler.PermissionClaimType
             || c.Type == PermissionAuthorizationHandler.LegacyPermissionClaimType)
            && string.Equals(c.Value, code.ToString(), StringComparison.OrdinalIgnoreCase));
}
