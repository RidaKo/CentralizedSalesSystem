using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Mappers;
using CentralizedSalesSystem.API.Models;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly CentralizedSalesDbContext _context;

        public OrderService(CentralizedSalesDbContext context)
        {
            _context = context;
        }

        // --------------------------------------------------
        // GET ALL
        // --------------------------------------------------
        public async Task<object> GetOrdersAsync(
            int page,
            int limit,
            string? sortBy = null,
            string? sortDirection = null,
            string? filterByStatus = null,
            DateTimeOffset? filterByUpdatedAt = null,
            long? filterByBusinessId = null,
            long? filterByReservationId = null,
            long? filterByTableId = null)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Table)
                .Include(o => o.Discount)
                .AsQueryable();

            // filters
            if (!string.IsNullOrEmpty(filterByStatus) &&
                Enum.TryParse<OrderStatus>(filterByStatus, out var parsedStatus))
                query = query.Where(o => o.Status == parsedStatus);

            if (filterByUpdatedAt.HasValue) query = query.Where(o => o.UpdatedAt >= filterByUpdatedAt.Value);
            if (filterByBusinessId.HasValue) query = query.Where(o => o.BusinessId == filterByBusinessId.Value);
            if (filterByReservationId.HasValue) query = query.Where(o => o.ReservationId == filterByReservationId.Value);
            if (filterByTableId.HasValue) query = query.Where(o => o.TableId == filterByTableId.Value);

            // sorting
            bool desc = sortDirection?.ToLower() == "desc";
            query = sortBy switch
            {
                "tip" => desc ? query.OrderByDescending(o => o.Tip) : query.OrderBy(o => o.Tip),
                "updatedAt" => desc ? query.OrderByDescending(o => o.UpdatedAt) : query.OrderBy(o => o.UpdatedAt),
                "status" => desc ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
                _ => query
            };

            int total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            // final pagination object (same as IOrderItemService)
            return new
            {
                data = items.Select(EntityToDtoMapper.ToOrderResponse),
                page,
                limit,
                total,
                totalPages = (int)Math.Ceiling(total / (double)limit)
            };
        }



        // --------------------------------------------------
        // GET BY ID
        // --------------------------------------------------
        public async Task<OrderResponseDto?> GetOrderByIdAsync(long id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Table)
                .Include(o => o.Discount)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order == null ? null : EntityToDtoMapper.ToOrderResponse(order);
        }

        // --------------------------------------------------
        // CREATE
        // --------------------------------------------------
        public async Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto dto)
        {
            Discount? discount = dto.DiscountId.HasValue
                ? await _context.Discounts.FindAsync(dto.DiscountId.Value)
                : null;

            if (dto.DiscountId.HasValue && discount == null)
                throw new Exception($"Discount {dto.DiscountId} does not exist");

            Table? table = dto.TableId.HasValue
                ? await _context.Tables.FindAsync(dto.TableId.Value)
                : null;

            if (dto.TableId.HasValue && table == null)
                throw new Exception($"Table {dto.TableId} does not exist");

            var order = new Order
            {
                BusinessId = dto.BusinessId,
                Tip = dto.Tip,
                Status = dto.Status,
                UserId = dto.UserId,
                TableId = dto.TableId,
                DiscountId = dto.DiscountId,
                ReservationId = dto.ReservationId,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return EntityToDtoMapper.ToOrderResponse(order);
        }

        // --------------------------------------------------
        // UPDATE
        // --------------------------------------------------
        public async Task<OrderResponseDto?> UpdateOrderAsync(long id, OrderUpdateDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return null;

            if (dto.BusinessId.HasValue) order.BusinessId = dto.BusinessId.Value;
            if (dto.Tip.HasValue) order.Tip = dto.Tip.Value;
            if (dto.Status.HasValue) order.Status = dto.Status.Value;
            if (dto.UserId.HasValue) order.UserId = dto.UserId.Value;
            if (dto.TableId.HasValue) order.TableId = dto.TableId.Value;
            if (dto.ReservationId.HasValue) order.ReservationId = dto.ReservationId.Value;
            if (dto.DiscountId.HasValue) order.DiscountId = dto.DiscountId.Value;

            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return EntityToDtoMapper.ToOrderResponse(order);
        }

        // --------------------------------------------------
        // DELETE
        // --------------------------------------------------
        public async Task<bool> DeleteOrderAsync(long id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
