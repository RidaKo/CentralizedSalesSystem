using CentralizedSalesSystem.API.Models.Orders.DTOs.ServiceChargeDTOs;
using CentralizedSalesSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CentralizedSalesSystem.API.Controllers.Orders
{
    [ApiController]
    [Route("serviceCharges")]
    public class ServiceChargesController : ControllerBase
    {
        private readonly IServiceChargeService _service;

        public ServiceChargesController(IServiceChargeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetServiceCharges(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] string? filterByName = null,
            [FromQuery] decimal? filterByRate = null,
            [FromQuery] long? filterByBusinessId = null)
        {
            var result = await _service.GetServiceChargesAsync(
                page, limit, sortBy, sortDirection, filterByName, filterByRate, filterByBusinessId
            );

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceChargeResponseDto>> GetServiceChargeById(long id)
        {
            var sc = await _service.GetServiceChargeByIdAsync(id);
            return sc == null ? NotFound() : Ok(sc);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceChargeResponseDto>> CreateServiceCharge(ServiceChargeCreateDto dto)
        {
            var sc = await _service.CreateServiceChargeAsync(dto);
            return CreatedAtAction(nameof(GetServiceChargeById), new { id = sc.Id }, sc);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<ServiceChargeResponseDto>> UpdateServiceCharge(long id, ServiceChargeUpdateDto dto)
        {
            var sc = await _service.UpdateServiceChargeAsync(id, dto);
            return sc == null ? NotFound() : Ok(sc);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceCharge(long id)
        {
            var deleted = await _service.DeleteServiceChargeAsync(id);
            return deleted
                ? Ok(new { message = "Successfully deleted service charge" })
                : NotFound();
        }
    }
}
