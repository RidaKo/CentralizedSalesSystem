using CentralizedSalesSystem.API.Authorization;
using CentralizedSalesSystem.API.Models.Auth.enums;
using CentralizedSalesSystem.API.Models.Orders.DTOs.TableDTOs;
using CentralizedSalesSystem.API.Services;
using CentralizedSalesSystem.API.Models.Orders.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralizedSalesSystem.API.Controllers.Orders
{
    [Route("tables")]
    [Authorize]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly ITableService _service;

        public TableController(ITableService service)
        {
            _service = service;
        }

        [HttpGet]
        [AuthorizePermission(nameof(PermissionCode.TABLE_VIEW))]
        public async Task<IActionResult> GetAll([
            FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] string? filterByName = null,
            [FromQuery] string? filterByStatus = null,
            [FromQuery] int? filterByCapacity = null,
            [FromQuery] long? filterByBusinessId = null)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 20;

            var result = await _service.GetAllAsync(page, limit, sortBy, sortDirection, filterByName, filterByStatus, filterByCapacity, filterByBusinessId);
            return Ok(result);
        }

        [HttpPost]
        [AuthorizePermission(nameof(PermissionCode.TABLE_MANAGE))]
        public async Task<IActionResult> Create([FromBody] TableCreateDto dto)
        {
            if (dto == null) return BadRequest();

            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { tableId = created.Id }, created);
        }

        [HttpGet("{tableId:long}")]
        [AuthorizePermission(nameof(PermissionCode.TABLE_VIEW))]
        public async Task<IActionResult> GetById([FromRoute] long tableId)
        {
            var t = await _service.GetByIdAsync(tableId);
            if (t == null) return NotFound();
            return Ok(t);
        }

        [HttpPatch("{tableId:long}")]
        [AuthorizePermission(nameof(PermissionCode.TABLE_MANAGE))]
        public async Task<IActionResult> Patch([FromRoute] long tableId, [FromBody] TablePatchDto dto)
        {
            if (dto == null) return BadRequest();

            var updated = await _service.PatchAsync(tableId, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{tableId:long}")]
        [AuthorizePermission(nameof(PermissionCode.TABLE_MANAGE))]
        public async Task<IActionResult> Delete([FromRoute] long tableId)
        {
            var ok = await _service.DeleteAsync(tableId);
            if (!ok) return NotFound();
            return Ok();
        }
    }
}
