using CentralizedSalesSystem.API.Authorization;
using CentralizedSalesSystem.API.Models.Auth.enums;
using CentralizedSalesSystem.API.Models.Orders.DTOs.TaxDTOs;
using CentralizedSalesSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CentralizedSalesSystem.API.Controllers.Orders
{
    [ApiController]
    [Authorize]
    [Route("taxes")]
    public class TaxesController : ControllerBase
    {
        private readonly ITaxService _service;

        public TaxesController(ITaxService service)
        {
            _service = service;
        }

        [HttpGet]
        [AuthorizePermission(nameof(PermissionCode.TAX_VIEW))]
        public async Task<ActionResult<object>> GetTaxes(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = null,
            [FromQuery] string? filterByName = null,
            [FromQuery] decimal? filterByRate = null,
            [FromQuery] DateTimeOffset? filterByCreationDate = null,
            [FromQuery] string? filterByActivity = null,
            [FromQuery] DateTimeOffset? filterByEffectiveFrom = null,
            [FromQuery] DateTimeOffset? filterByEffectiveTo = null,
            [FromQuery] long? filterByBusinessId = null)
        {
            var result = await _service.GetTaxesAsync(
                page, limit, sortBy, sortDirection,
                filterByName, filterByRate, filterByCreationDate,
                filterByActivity, filterByEffectiveFrom, filterByEffectiveTo,
                filterByBusinessId
            );

            return Ok(result);
        }

        [HttpGet("{id}")]
        [AuthorizePermission(nameof(PermissionCode.TAX_VIEW))]
        public async Task<ActionResult<TaxResponseDto>> GetTaxById(long id)
        {
            var tax = await _service.GetTaxByIdAsync(id);
            return tax == null ? NotFound() : Ok(tax);
        }

        [HttpPost]
        [AuthorizePermission(nameof(PermissionCode.TAX_MANAGE))]
        public async Task<ActionResult<TaxResponseDto>> CreateTax(TaxCreateDto dto)
        {
            var created = await _service.CreateTaxAsync(dto);
            return created == null ? BadRequest() : Ok(created);
        }

        [HttpPatch("{id}")]
        [AuthorizePermission(nameof(PermissionCode.TAX_MANAGE))]
        public async Task<ActionResult<TaxResponseDto>> UpdateTax(long id, TaxUpdateDto dto)
        {
            var updated = await _service.UpdateTaxAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        [AuthorizePermission(nameof(PermissionCode.TAX_MANAGE))]
        public async Task<IActionResult> DeleteTax(long id)
        {
            var deleted = await _service.DeleteTaxAsync(id);
            return deleted ? Ok(new { message = "Successfully deleted tax" }) : NotFound();
        }
    }
}
