using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ServiceChargeDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Controllers.Orders
{
    [ApiController]
    [Route("serviceCharges")]
    public class ServiceChargesController : ControllerBase
    {
        private readonly CentralizedSalesDbContext _context;

        public ServiceChargesController(CentralizedSalesDbContext context)
        {
            _context = context;
        }

        // GET: /serviceCharges
        [HttpGet]
        public async Task<ActionResult<object>> GetServiceCharges([FromQuery] int page = 1, [FromQuery] int limit = 20)
        {
            var query = _context.ServiceCharges.AsQueryable();

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)total / limit);

            var serviceCharges = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var result = serviceCharges.Select(sc => new ServiceChargeReadDto
            {
                Id = sc.Id,
                Name = sc.Name,
                Rate = sc.rate,
                CreatedAt = sc.CreatedAt,
                UpdatedAt = sc.UpdatedAt,
                BusinessId = sc.BusinessId
            });

            return Ok(new
            {
                data = result,
                page,
                limit,
                total,
                totalPages
            });
        }

        // GET: /serviceCharges/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceChargeReadDto>> GetServiceChargeById(long id)
        {
            var sc = await _context.ServiceCharges.FindAsync(id);
            if (sc == null) return NotFound();

            return Ok(new ServiceChargeReadDto
            {
                Id = sc.Id,
                Name = sc.Name,
                Rate = sc.rate,
                CreatedAt = sc.CreatedAt,
                UpdatedAt = sc.UpdatedAt,
                BusinessId = sc.BusinessId
            });
        }

        // POST: /serviceCharges
        [HttpPost]
        public async Task<ActionResult<ServiceChargeReadDto>> CreateServiceCharge(ServiceChargeCreateDto dto)
        {
            var sc = new ServiceCharge
            {
                Name = dto.Name,
                rate = dto.Rate,
                BusinessId = dto.BusinessId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.ServiceCharges.Add(sc);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetServiceChargeById), new { id = sc.Id }, new ServiceChargeReadDto
            {
                Id = sc.Id,
                Name = sc.Name,
                Rate = sc.rate,
                CreatedAt = sc.CreatedAt,
                UpdatedAt = sc.UpdatedAt,
                BusinessId = sc.BusinessId
            });
        }

        // PATCH: /serviceCharges/{id}
        [HttpPatch("{id}")]
        public async Task<ActionResult<ServiceChargeReadDto>> UpdateServiceCharge(long id, ServiceChargeUpdateDto dto)
        {
            var sc = await _context.ServiceCharges.FindAsync(id);
            if (sc == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.Name)) sc.Name = dto.Name;
            if (dto.Rate.HasValue) sc.rate = dto.Rate.Value;

            sc.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ServiceChargeReadDto
            {
                Id = sc.Id,
                Name = sc.Name,
                Rate = sc.rate,
                CreatedAt = sc.CreatedAt,
                UpdatedAt = sc.UpdatedAt,
                BusinessId = sc.BusinessId
            });
        }

        // DELETE: /serviceCharges/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceCharge(long id)
        {
            var sc = await _context.ServiceCharges.FindAsync(id);
            if (sc == null) return NotFound();

            _context.ServiceCharges.Remove(sc);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully deleted service charge" });
        }
    }
}
