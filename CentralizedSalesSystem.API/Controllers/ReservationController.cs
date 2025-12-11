using CentralizedSalesSystem.API.Models.Reservations;
using CentralizedSalesSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralizedSalesSystem.API.Controllers
{
    [Route("reservations")]
    [Authorize]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _service;

        public ReservationController(IReservationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([
            FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? sortBy = "createdAt",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] string? filterByName = null,
            [FromQuery] string? filterByPhone = null,
            [FromQuery] DateTimeOffset? filterByAppointmentTime = null,
            [FromQuery] DateTimeOffset? filterByCreationTime = null,
            [FromQuery] string? filterByStatus = null,
            [FromQuery] long? filterByBusinessId = null,
            [FromQuery] long? filterByUserId = null,
            [FromQuery] long? filterByTableId = null)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 20;

            var result = await _service.GetAllAsync(page, limit, sortBy, sortDirection, filterByName, filterByPhone, filterByAppointmentTime, filterByCreationTime, filterByStatus, filterByBusinessId, filterByUserId, filterByTableId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReservationCreateDto dto)
        {
            if (dto == null) return BadRequest();
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { reservationId = created.Id }, created);
        }

        [HttpGet("{reservationId:long}")]
        public async Task<IActionResult> GetById([FromRoute] long reservationId)
        {
            var r = await _service.GetByIdAsync(reservationId);
            if (r == null) return NotFound();
            return Ok(r);
        }

        [HttpPatch("{reservationId:long}")]
        public async Task<IActionResult> Patch([FromRoute] long reservationId, [FromBody] ReservationPatchDto dto)
        {
            if (dto == null) return BadRequest();
            var updated = await _service.PatchAsync(reservationId, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{reservationId:long}")]
        public async Task<IActionResult> Delete([FromRoute] long reservationId)
        {
            var ok = await _service.DeleteAsync(reservationId);
            if (!ok) return NotFound();
            return Ok();
        }
    }
}