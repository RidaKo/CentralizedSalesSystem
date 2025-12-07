using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.DiscountDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Controllers.Orders
{
    [ApiController]
    [Route("discounts")]
    public class DiscountsController : ControllerBase
    {
        private readonly CentralizedSalesDbContext _context;

        public DiscountsController(CentralizedSalesDbContext context)
        {
            _context = context;
        }

        // GET: /discounts
        [HttpGet]
        public async Task<ActionResult<object>> GetDiscounts(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20
        )
        {
            var query = _context.Discounts.AsQueryable();

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)total / limit);

            var discounts = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var result = discounts.Select(d => new DiscountResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                Rate = d.rate,
                ValidFrom = d.ValidFrom,
                ValidTo = d.ValidTo,
                Type = d.Type,
                AppliesTo = d.AppliesTo,
                Status = d.Status,
                BusinessId = d.BusinessId
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

        // GET: /discounts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DiscountResponseDto>> GetDiscountById(long id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            return Ok(new DiscountResponseDto
            {
                Id = discount.Id,
                Name = discount.Name,
                Rate = discount.rate,
                ValidFrom = discount.ValidFrom,
                ValidTo = discount.ValidTo,
                Type = discount.Type,
                AppliesTo = discount.AppliesTo,
                Status = discount.Status,
                BusinessId = discount.BusinessId
            });
        }

        // POST: /discounts
        [HttpPost]
        public async Task<ActionResult<DiscountResponseDto>> CreateDiscount(DiscountCreateDto dto)
        {
            var discount = new Discount
            {
                Name = dto.Name,
                rate = dto.Rate,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                Type = dto.Type,
                AppliesTo = dto.AppliesTo,
                Status = dto.Status,
                BusinessId = dto.BusinessId
            };

            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDiscountById), new { id = discount.Id },
                new DiscountResponseDto
                {
                    Id = discount.Id,
                    Name = discount.Name,
                    Rate = discount.rate,
                    ValidFrom = discount.ValidFrom,
                    ValidTo = discount.ValidTo,
                    Type = discount.Type,
                    AppliesTo = discount.AppliesTo,
                    Status = discount.Status,
                    BusinessId = discount.BusinessId
                });
        }

        // PATCH: /discounts/{id}
        [HttpPatch("{id}")]
        public async Task<ActionResult<DiscountResponseDto>> UpdateDiscount(long id, DiscountUpdateDto dto)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.Name)) discount.Name = dto.Name;
            if (dto.Rate.HasValue) discount.rate = dto.Rate.Value;
            if (dto.ValidFrom.HasValue) discount.ValidFrom = dto.ValidFrom.Value;
            if (dto.ValidTo.HasValue) discount.ValidTo = dto.ValidTo.Value;
            if (dto.Type.HasValue) discount.Type = dto.Type.Value;
            if (dto.AppliesTo.HasValue) discount.AppliesTo = dto.AppliesTo.Value;
            if (dto.Status.HasValue) discount.Status = dto.Status.Value;

            await _context.SaveChangesAsync();

            return Ok(new DiscountResponseDto
            {
                Id = discount.Id,
                Name = discount.Name,
                Rate = discount.rate,
                ValidFrom = discount.ValidFrom,
                ValidTo = discount.ValidTo,
                Type = discount.Type,
                AppliesTo = discount.AppliesTo,
                Status = discount.Status,
                BusinessId = discount.BusinessId
            });
        }

        // DELETE: /discounts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiscount(long id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully deleted discount" });
        }
    }
}
