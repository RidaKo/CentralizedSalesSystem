using CentralizedSalesSystem.API.Models.Orders.DTOs.RefundDTOs;
using CentralizedSalesSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace CentralizedSalesSystem.API.Controllers.Orders
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class RefundsController : ControllerBase
    {
        private readonly IRefundService _service;

        public RefundsController(IRefundService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetRefunds([FromQuery] int page = 1, [FromQuery] int limit = 20,
            [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
        {
            var refunds = await _service.GetRefundsAsync(page, limit, sortBy, sortDirection);
            return Ok(refunds);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRefund(long id)
        {
            var refund = await _service.GetRefundByIdAsync(id);
            if (refund == null) return NotFound();
            return Ok(refund);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRefund([FromBody] RefundCreateDto dto)
        {
            var created = await _service.CreateRefundAsync(dto);
            return CreatedAtAction(nameof(GetRefund), new { id = created.Id }, created);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateRefund(long id, [FromBody] RefundUpdateDto dto)
        {
            var updated = await _service.UpdateRefundAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRefund(long id)
        {
            var deleted = await _service.DeleteRefundAsync(id);
            return deleted ? Ok(new { message = "Refund deleted successfully" }) : NotFound();
        }
    }
}
