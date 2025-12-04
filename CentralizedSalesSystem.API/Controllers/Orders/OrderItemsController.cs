using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.DiscountDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.TaxDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ServiceChargeDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderItemDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Controllers.Orders
{
    [ApiController]
    [Route("orderItems")]
    public class OrderItemsController : ControllerBase
    {
        private readonly CentralizedSalesDbContext _context;

        public OrderItemsController(CentralizedSalesDbContext context)
        {
            _context = context;
        }

        // GET: /orderItems
        [HttpGet]
        public async Task<ActionResult<object>> GetOrderItems([FromQuery] int page = 1, [FromQuery] int limit = 20)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Item)
                .Include(oi => oi.Discount)
                .Include(oi => oi.Taxes)
                .Include(oi => oi.ServiceCharge)
                .AsQueryable();

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)total / limit);

            var items = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var result = items.Select(oi => new OrderItemReadDto
            {
                Id = oi.Id,
                Quantity = oi.Quantity,
                Notes = oi.Notes,
                ItemId = oi.ItemId,
                DiscountId = oi.DiscountId,
                Item = oi.Item != null ? new ItemReadDto
                {
                    Id = oi.Item.Id,
                    Name = oi.Item.Name,
                    Description = oi.Item.Description,
                    Price = oi.Item.Price,
                    Type = oi.Item.Type,
                    Stock = oi.Item.Stock,
                    BusinessId = oi.Item.BusinessId,
                    Variations = oi.Item.Variations?.Select(v => new ItemVariationReadDto
                    {
                        Id = v.Id,
                        Name = v.Name,
                        ItemId = v.ItemId,
                        Selection = v.Selection
                    }).ToList() ?? new List<ItemVariationReadDto>()
                } : null,
                Discount = oi.Discount != null ? new DiscountReadDto
                {
                    Id = oi.Discount.Id,
                    Name = oi.Discount.Name,
                    Rate = oi.Discount.rate,
                    ValidFrom = oi.Discount.ValidFrom,
                    ValidTo = oi.Discount.ValidTo,
                    Type = oi.Discount.Type,
                    AppliesTo = oi.Discount.AppliesTo,
                    Status = oi.Discount.Status,
                    BusinessId = oi.Discount.BusinessId
                } : null,
                Taxes = oi.Taxes?.Select(t => new TaxReadDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Rate = t.Rate,
                    BusinessId = t.BusinessId
                }).ToList() ?? new List<TaxReadDto>(),
                ServiceCharges = oi.ServiceCharge?.Select(sc => new ServiceChargeReadDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Rate = sc.rate,
                    BusinessId = sc.BusinessId
                }).ToList() ?? new List<ServiceChargeReadDto>()
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

        // GET: /orderItems/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderItemReadDto>> GetOrderItemById(long id)
        {
            var oi = await _context.OrderItems
                .Include(oi => oi.Item)
                .Include(oi => oi.Discount)
                .Include(oi => oi.Taxes)
                .Include(oi => oi.ServiceCharge)
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (oi == null) return NotFound();

            return Ok(new OrderItemReadDto
            {
                Id = oi.Id,
                Quantity = oi.Quantity,
                Notes = oi.Notes,
                ItemId = oi.ItemId,
                DiscountId = oi.DiscountId,
                Item = oi.Item != null ? new ItemReadDto
                {
                    Id = oi.Item.Id,
                    Name = oi.Item.Name,
                    Description = oi.Item.Description,
                    Price = oi.Item.Price,
                    Type = oi.Item.Type,
                    Stock = oi.Item.Stock,
                    BusinessId = oi.Item.BusinessId,
                    Variations = oi.Item.Variations?.Select(v => new ItemVariationReadDto
                    {
                        Id = v.Id,
                        Name = v.Name,
                        ItemId = v.ItemId,
                        Selection = v.Selection
                    }).ToList() ?? new List<ItemVariationReadDto>()
                } : null,
                Discount = oi.Discount != null ? new DiscountReadDto
                {
                    Id = oi.Discount.Id,
                    Name = oi.Discount.Name,
                    Rate = oi.Discount.rate,
                    ValidFrom = oi.Discount.ValidFrom,
                    ValidTo = oi.Discount.ValidTo,
                    Type = oi.Discount.Type,
                    AppliesTo = oi.Discount.AppliesTo,
                    Status = oi.Discount.Status,
                    BusinessId = oi.Discount.BusinessId
                } : null,
                Taxes = oi.Taxes?.Select(t => new TaxReadDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Rate = t.Rate,
                    BusinessId = t.BusinessId
                }).ToList() ?? new List<TaxReadDto>(),
                ServiceCharges = oi.ServiceCharge?.Select(sc => new ServiceChargeReadDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Rate = sc.rate,
                    BusinessId = sc.BusinessId
                }).ToList() ?? new List<ServiceChargeReadDto>()
            });
        }

        // POST: /orderItems
        [HttpPost]
        public async Task<ActionResult<OrderItemReadDto>> CreateOrderItem(OrderItemCreateDto dto)
        {
            var item = await _context.Items.FindAsync(dto.ItemId);
            if (item == null) return BadRequest("Item not found.");

            var orderItem = new OrderItem
            {
                Quantity = dto.Quantity,
                Notes = dto.Notes,
                ItemId = dto.ItemId,
                DiscountId = dto.DiscountId
            };

            if (dto.TaxIds != null)
            {
                orderItem.Taxes = await _context.Taxes
                    .Where(t => dto.TaxIds.Contains(t.Id))
                    .ToListAsync();
            }

            if (dto.ServiceChargeIds != null)
            {
                orderItem.ServiceCharge = await _context.ServiceCharges
                    .Where(sc => dto.ServiceChargeIds.Contains(sc.Id))
                    .ToListAsync();
            }

            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();

            return await GetOrderItemById(orderItem.Id);
        }

        // PATCH: /orderItems/{id}
        [HttpPatch("{id}")]
        public async Task<ActionResult<OrderItemReadDto>> UpdateOrderItem(long id, OrderItemUpdateDto dto)
        {
            var orderItem = await _context.OrderItems
                .Include(oi => oi.Taxes)
                .Include(oi => oi.ServiceCharge)
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (orderItem == null) return NotFound();

            if (dto.Quantity.HasValue) orderItem.Quantity = dto.Quantity.Value;
            if (!string.IsNullOrEmpty(dto.Notes)) orderItem.Notes = dto.Notes;
            if (dto.DiscountId.HasValue) orderItem.DiscountId = dto.DiscountId;

            if (dto.TaxIds != null)
            {
                orderItem.Taxes = await _context.Taxes
                    .Where(t => dto.TaxIds.Contains(t.Id))
                    .ToListAsync();
            }

            if (dto.ServiceChargeIds != null)
            {
                orderItem.ServiceCharge = await _context.ServiceCharges
                    .Where(sc => dto.ServiceChargeIds.Contains(sc.Id))
                    .ToListAsync();
            }

            await _context.SaveChangesAsync();

            return await GetOrderItemById(id);
        }

        // DELETE: /orderItems/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderItem(long id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null) return NotFound();

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully deleted order item" });
        }
    }
}
