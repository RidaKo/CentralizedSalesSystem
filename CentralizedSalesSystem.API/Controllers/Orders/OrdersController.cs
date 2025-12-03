using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Controllers.Orders
{
    [ApiController]
    [Route("orders")]
    public class OrdersController : ControllerBase
    {
        private readonly CentralizedSalesDbContext _context;

        public OrdersController(CentralizedSalesDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------
        // GET: /orders
        // ---------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<object>> GetOrders(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] string? filterByStatus = null,
            [FromQuery] DateTimeOffset? filterByUpdatedAt = null,
            [FromQuery] long? filterByBusinessId = null,
            [FromQuery] long? filterByReservationId = null,
            [FromQuery] long? filterByTableId = null)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Table)
                .Include(o => o.Discount)
                .AsQueryable();

            // -------- FILTERS --------
            if (!string.IsNullOrEmpty(filterByStatus) &&
                Enum.TryParse<OrderStatus>(filterByStatus, out var status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (filterByUpdatedAt.HasValue)
                query = query.Where(o => o.UpdatedAt >= filterByUpdatedAt.Value);

            if (filterByBusinessId.HasValue)
                query = query.Where(o => o.BusinessId == filterByBusinessId.Value);

            if (filterByReservationId.HasValue)
                query = query.Where(o => o.ReservationId == filterByReservationId.Value);

            if (filterByTableId.HasValue)
                query = query.Where(o => o.TableId == filterByTableId.Value);

            // -------- SORTING --------
            bool desc = sortDirection?.ToLower() == "desc";

            query = sortBy switch
            {
                "tip" => desc ? query.OrderByDescending(o => o.Tip)
                                     : query.OrderBy(o => o.Tip),

                "updatedAt" => desc ? query.OrderByDescending(o => o.UpdatedAt)
                                     : query.OrderBy(o => o.UpdatedAt),

                "status" => desc ? query.OrderByDescending(o => o.Status)
                                     : query.OrderBy(o => o.Status),

                _ => query
            };

            // -------- PAGINATION --------
            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)total / limit);

            var orders = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var result = orders.Select(o => new OrderReadDto
            {
                Id = o.Id,
                BusinessId = o.BusinessId,
                Tip = o.Tip,
                UpdatedAt = o.UpdatedAt,
                Status = o.Status,
                UserId = o.UserId,
                TableId = o.TableId,
                DiscountId = o.DiscountId,
                ReservationId = o.ReservationId
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

        // ---------------------------------------------------------
        // GET: /orders/{orderId}
        // ---------------------------------------------------------
        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderReadDto>> GetOrderById(long orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Table)
                .Include(o => o.Discount)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            return Ok(new OrderReadDto
            {
                Id = order.Id,
                BusinessId = order.BusinessId,
                Tip = order.Tip,
                UpdatedAt = order.UpdatedAt,
                Status = order.Status,
                UserId = order.UserId,
                TableId = order.TableId,
                DiscountId = order.DiscountId,
                ReservationId = order.ReservationId
            });
        }

        // ---------------------------------------------------------
        // POST: /orders
        // ---------------------------------------------------------
        [HttpPost]
        public async Task<ActionResult<OrderReadDto>> CreateOrder(OrderCreateDto dto)
        {
            // Validate optional foreign keys
            Discount? discount = null;
            if (dto.DiscountId.HasValue)
            {
                discount = await _context.Discounts.FindAsync(dto.DiscountId.Value);
                if (discount == null)
                    return BadRequest($"Discount with Id {dto.DiscountId.Value} does not exist.");
            }

            Table? table = null;
            if (dto.TableId.HasValue)
            {
                table = await _context.Tables.FindAsync(dto.TableId.Value);
                if (table == null)
                    return BadRequest($"Table with Id {dto.TableId.Value} does not exist.");
            }

            var order = new Order
            {
                BusinessId = dto.BusinessId,
                Tip = dto.Tip,
                Status = dto.Status,
                UserId = dto.UserId,
                TableId = dto.TableId,
                ReservationId = dto.ReservationId,
                DiscountId = dto.DiscountId,
                Discount = discount,
                Table = table,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var result = new OrderReadDto
            {
                Id = order.Id,
                BusinessId = order.BusinessId,
                Tip = order.Tip,
                UpdatedAt = order.UpdatedAt,
                Status = order.Status,
                UserId = order.UserId,
                TableId = order.TableId,
                DiscountId = order.DiscountId,
                ReservationId = order.ReservationId
            };

            return CreatedAtAction(nameof(GetOrderById), new { orderId = order.Id }, result);
        }


        // ---------------------------------------------------------
        // PATCH: /orders/{orderId}
        // ---------------------------------------------------------
        [HttpPatch("{orderId}")]
        public async Task<ActionResult<OrderReadDto>> ModifyOrder(long orderId, OrderUpdateDto dto)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            // Apply only provided fields
            if (dto.BusinessId.HasValue) order.BusinessId = dto.BusinessId.Value;
            if (dto.Tip.HasValue) order.Tip = dto.Tip.Value;
            if (dto.Status.HasValue) order.Status = dto.Status.Value;
            if (dto.UserId.HasValue) order.UserId = dto.UserId.Value;
            if (dto.TableId.HasValue) order.TableId = dto.TableId.Value;
            if (dto.ReservationId.HasValue) order.ReservationId = dto.ReservationId.Value;
            if (dto.DiscountId.HasValue) order.DiscountId = dto.DiscountId.Value;

            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            var result = new OrderReadDto
            {
                Id = order.Id,
                BusinessId = order.BusinessId,
                Tip = order.Tip,
                UpdatedAt = order.UpdatedAt,
                Status = order.Status,
                UserId = order.UserId,
                TableId = order.TableId,
                DiscountId = order.DiscountId,
                ReservationId = order.ReservationId
            };

            return Ok(result);
        }

        // ---------------------------------------------------------
        // DELETE: /orders/{orderId}
        // ---------------------------------------------------------
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(long orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully deleted order" });
        }
    }
}
