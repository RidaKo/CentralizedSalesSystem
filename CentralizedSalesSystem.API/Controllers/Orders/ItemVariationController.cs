using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Controllers.Orders
{
    [ApiController]
    [Route("itemVariations")]
    public class ItemVariationsController : ControllerBase
    {
        private readonly CentralizedSalesDbContext _context;

        public ItemVariationsController(CentralizedSalesDbContext context)
        {
            _context = context;
        }

        // GET: /itemVariations
        [HttpGet]
        public async Task<ActionResult<object>> GetItemVariations(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20
        )
        {
            var query = _context.ItemVariations.AsQueryable();

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)total / limit);

            var variations = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .Include(v => v.Item)
                .ToListAsync();

            var result = variations.Select(v => new ItemVariationReadDto
            {
                Id = v.Id,
                Name = v.Name,
                ItemId = v.ItemId,
                Selection = v.Selection
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

        // GET: /itemVariations/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemVariationReadDto>> GetItemVariationById(long id)
        {
            var variation = await _context.ItemVariations.FindAsync(id);
            if (variation == null) return NotFound();

            return Ok(new ItemVariationReadDto
            {
                Id = variation.Id,
                Name = variation.Name,
                ItemId = variation.ItemId,
                Selection = variation.Selection
            });
        }

        // POST: /itemVariations
        [HttpPost]
        public async Task<ActionResult<ItemVariationReadDto>> CreateItemVariation(ItemVariationCreateDto dto)
        {
            var variation = new ItemVariation
            {
                Name = dto.Name,
                ItemId = dto.ItemId,
                Selection = dto.Selection
            };

            _context.ItemVariations.Add(variation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItemVariationById), new { id = variation.Id },
                new ItemVariationReadDto
                {
                    Id = variation.Id,
                    Name = variation.Name,
                    ItemId = variation.ItemId,
                    Selection = variation.Selection
                });
        }

        // PATCH: /itemVariations/{id}
        [HttpPatch("{id}")]
        public async Task<ActionResult<ItemVariationReadDto>> UpdateItemVariation(long id, ItemVariationUpdateDto dto)
        {
            var variation = await _context.ItemVariations.FindAsync(id);
            if (variation == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.Name)) variation.Name = dto.Name;
            if (dto.ItemId.HasValue) variation.ItemId = dto.ItemId.Value;
            if (dto.Selection.HasValue) variation.Selection = dto.Selection.Value;

            await _context.SaveChangesAsync();

            return Ok(new ItemVariationReadDto
            {
                Id = variation.Id,
                Name = variation.Name,
                ItemId = variation.ItemId,
                Selection = variation.Selection
            });
        }

        // DELETE: /itemVariations/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItemVariation(long id)
        {
            var variation = await _context.ItemVariations.FindAsync(id);
            if (variation == null) return NotFound();

            _context.ItemVariations.Remove(variation);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully deleted item variation" });
        }
    }
}
