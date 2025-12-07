using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;
using CentralizedSalesSystem.API.Models.Auth;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Controllers.Items
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly CentralizedSalesDbContext _context;

        public ItemsController(CentralizedSalesDbContext context)
        {
            _context = context;
        }

        // GET: /items
        [HttpGet]
        public async Task<ActionResult<object>> GetItems(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20
        )
        {
            var query = _context.Items.AsQueryable();

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)total / limit);

            var items = await query
                .Include(i => i.Variations)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();


            var result = items.Select(i => new ItemResponseDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Price = i.Price,
                Stock = i.Stock,
                Type = i.Type,
                BusinessId = i.BusinessId,
                //Creating a list for ItemVariations from DTO
                Variations = i.Variations.Select(v => new ItemVariationResponseDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    ItemId = v.ItemId,
                    Selection = v.Selection
                }).ToList(),
                //Creating a list for Roles from model
                AssociatedRoles = i.Type == ItemType.service
                ? i.AssociatedRoles.Select(v => new Role
                {
                    Id = v.Id,
                    BussinessId = v.BussinessId,
                    Title = v.Title,
                    Description = v.Description,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt,
                    Status = v.Status
                }).ToList()
                : null
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

        // GET: /items/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemResponseDto>> GetItemById(long id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            return Ok(new ItemResponseDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Stock = item.Stock,
                Type = item.Type,
                BusinessId = item.BusinessId
            });
        }

        // POST: /items
        [HttpPost]
        public async Task<ActionResult<ItemResponseDto>> CreateItem(ItemCreateDto dto)
        {
            var item = new Item
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                Type = dto.Type,
                BusinessId = dto.BusinessId
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItemById), new { id = item.Id },
                new ItemResponseDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.Price,
                    Stock = item.Stock,
                    Type = item.Type,
                    BusinessId = item.BusinessId
                });
        }

        // PATCH: /items/{id}
        [HttpPatch("{id}")]
        public async Task<ActionResult<ItemResponseDto>> UpdateItem(long id, ItemUpdateDto dto)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.Name)) item.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description)) item.Description = dto.Description;
            if (dto.Price.HasValue) item.Price = dto.Price.Value;
            if (dto.Stock.HasValue) item.Stock = dto.Stock.Value;
            if (dto.Type.HasValue) item.Type = dto.Type.Value;
            if (dto.BusinessId.HasValue) item.BusinessId = dto.BusinessId.Value;

            await _context.SaveChangesAsync();

            return Ok(new ItemResponseDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Stock = item.Stock,
                Type = item.Type,
                BusinessId = item.BusinessId
            });
        }

        // DELETE: /items/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(long id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully deleted item" });
        }
    }
}
