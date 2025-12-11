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
        private readonly CentralizedSalesDbContext _db;

        public OrderService(CentralizedSalesDbContext db)
        {
            _db = db;
        }

        // --------------------------------------------------
        // GET ALL ORDERS
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
            var query = _db.Orders
                .Include(o => o.Discount)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Item)
                        .ThenInclude(i => i.Variations)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Discount)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Tax)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.ServiceCharge)
                .Include(o => o.Payments)
                .AsQueryable();

            // Filters
            if (!string.IsNullOrEmpty(filterByStatus) &&
                Enum.TryParse<OrderStatus>(filterByStatus, out var parsedStatus))
                query = query.Where(o => o.Status == parsedStatus);

            if (filterByUpdatedAt.HasValue) query = query.Where(o => o.UpdatedAt >= filterByUpdatedAt.Value);
            if (filterByBusinessId.HasValue) query = query.Where(o => o.BusinessId == filterByBusinessId.Value);
            if (filterByReservationId.HasValue) query = query.Where(o => o.ReservationId == filterByReservationId.Value);
            if (filterByTableId.HasValue) query = query.Where(o => o.TableId == filterByTableId.Value);

            // Sorting
            bool desc = sortDirection?.ToLower() == "desc";
            query = sortBy switch
            {
                "tip" => desc ? query.OrderByDescending(o => o.Tip) : query.OrderBy(o => o.Tip),
                "updatedAt" => desc ? query.OrderByDescending(o => o.UpdatedAt) : query.OrderBy(o => o.UpdatedAt),
                "status" => desc ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
                _ => query
            };

            // Pagination
            var total = await query.CountAsync();
            var orders = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            // Map to DTOs and calculate totals
            var result = orders.Select(o =>
            {
                var dto = o.ToOrderResponse();
                dto.Items = o.Items.Select(i => i.ToDto()).ToList();
                CalculateOrderTotals(o, dto);
                return dto;
            }).ToList();

            return new
            {
                data = result,
                page,
                limit,
                total,
                totalPages = (int)Math.Ceiling(total / (double)limit)
            };
        }

        // --------------------------------------------------
        // GET ORDER BY ID
        // --------------------------------------------------
        public async Task<OrderResponseDto?> GetOrderByIdAsync(long id)
        {
            var order = await _db.Orders
                .Include(o => o.Discount)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Item)
                        .ThenInclude(i => i.Variations)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Discount)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Tax)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.ServiceCharge)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            var dto = order.ToOrderResponse();
            dto.Items = order.Items.Select(i => i.ToDto()).ToList();
            CalculateOrderTotals(order, dto);
            return dto;
        }

        // --------------------------------------------------
        // CREATE ORDER
        // --------------------------------------------------
        public async Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto dto)
        {
            // Load discount if provided
            Discount? discount = null;
            if (dto.DiscountId.HasValue)
            {
                discount = await _db.Discounts.FindAsync(dto.DiscountId.Value);
                if (discount == null)
                    throw new Exception($"Discount {dto.DiscountId} does not exist");
            }

            // Create order without items
            var order = new Order
            {
                BusinessId = dto.BusinessId,
                Tip = dto.Tip,
                Status = dto.Status,
                UserId = dto.UserId,
                TableId = dto.TableId,
                ReservationId = dto.ReservationId,
                DiscountId = discount?.Id,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            // Assign order-level discount
            if (discount != null && discount.AppliesTo == DiscountAppliesTo.Order)
                order.Discount = discount;

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Reload order fully with relationships for correct totals
            var loadedOrder = await _db.Orders
                .Include(o => o.Discount)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Item)
                        .ThenInclude(it => it.Variations)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Discount)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Tax)
                .Include(o => o.Items)
                    .ThenInclude(i => i.ServiceCharge)
                .FirstAsync(o => o.Id == order.Id);

            var dtoResponse = loadedOrder.ToOrderResponse();
            dtoResponse.Items = loadedOrder.Items.Select(i => i.ToDto()).ToList();
            CalculateOrderTotals(loadedOrder, dtoResponse);

            return dtoResponse;
        }

        // --------------------------------------------------
        // UPDATE ORDER
        // --------------------------------------------------
        public async Task<OrderResponseDto?> UpdateOrderAsync(long id, OrderUpdateDto dto)
        {
            var order = await _db.Orders
                .Include(o => o.Discount)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Item)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Discount)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Tax)
                .Include(o => o.Items)
                    .ThenInclude(i => i.ServiceCharge)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            if (order.Status == OrderStatus.Closed)
                throw new InvalidOperationException("This order cannot be updated because it is closed");

            if (dto.BusinessId.HasValue) order.BusinessId = dto.BusinessId.Value;
            if (dto.Tip.HasValue) order.Tip = dto.Tip.Value;
            if (dto.Status.HasValue) order.Status = dto.Status.Value;
            if (dto.UserId.HasValue) order.UserId = dto.UserId.Value;
            if (dto.TableId.HasValue) order.TableId = dto.TableId.Value;
            if (dto.ReservationId.HasValue) order.ReservationId = dto.ReservationId.Value;
            if (dto.DiscountId.HasValue)
            {
                var discount = await _db.Discounts.FindAsync(dto.DiscountId.Value);
                if (discount != null && discount.AppliesTo == DiscountAppliesTo.Order)
                {
                    order.Discount = discount;
                    order.DiscountId = discount.Id;
                }
            }

            order.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

            var dtoResponse = order.ToOrderResponse();
            dtoResponse.Items = order.Items.Select(i => i.ToDto()).ToList();
            CalculateOrderTotals(order, dtoResponse);
            return dtoResponse;
        }

        // --------------------------------------------------
        // DELETE ORDER
        // --------------------------------------------------
        public async Task<bool> DeleteOrderAsync(long id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return false;

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
            return true;
        }

        // --------------------------------------------------
        // CALCULATE TOTALS
        // --------------------------------------------------
        public void CalculateOrderTotals(Order order, OrderResponseDto dto)
        {
            // Subtotal: sum of item base prices × quantity
            dto.Subtotal = order.Items.Sum(i => i.Quantity * i.Item.Price);

            // Item-level discount
            // Item-level discount (supports product and service types)
            decimal itemLevelDiscount = order.Items.Sum(i =>
            {
                if (i.Discount == null) return 0m;

                bool applies =
                    (i.Discount.AppliesTo == DiscountAppliesTo.Product && i.Item.Type == ItemType.Product) ||
                    (i.Discount.AppliesTo == DiscountAppliesTo.Service && i.Item.Type == ItemType.Service);

                if (!applies)
                    return 0m;

                return (i.Discount.rate / 100m) * i.Quantity * i.Item.Price;
            });


            // Order-level discount
            decimal orderLevelDiscount = order.Discount != null && order.Discount.AppliesTo == DiscountAppliesTo.Order
                ? (order.Discount.rate / 100m) * dto.Subtotal
                : 0m;

            dto.DiscountTotal = itemLevelDiscount + orderLevelDiscount;

            // Tax — apply after item-level discount
            dto.TaxTotal = order.Items.Sum(i =>
            {
                if (i.Tax == null || i.Tax.Status != TaxStatus.Active ||
                    i.Tax.EffectiveFrom > DateTimeOffset.UtcNow ||
                    (i.Tax.EffectiveTo.HasValue && i.Tax.EffectiveTo.Value < DateTimeOffset.UtcNow))
                    return 0m;

                // Calculate the taxable amount after item-level discount
                decimal itemDiscount = i.Discount != null && i.Discount.AppliesTo == DiscountAppliesTo.Product
                    ? (i.Discount.rate / 100m) * i.Quantity * i.Item.Price
                    : 0m;

                decimal taxableAmount = i.Quantity * i.Item.Price - itemDiscount;

                return (i.Tax.Rate / 100m) * taxableAmount;
            });

            // Service charge — apply after item-level discount
            dto.ServiceChargeTotal = order.Items.Sum(i =>
            {
                if (i.ServiceCharge == null)
                    return 0m;

                decimal itemDiscount = i.Discount != null && i.Discount.AppliesTo == DiscountAppliesTo.Product
                    ? (i.Discount.rate / 100m) * i.Quantity * i.Item.Price
                    : 0m;

                decimal baseAmount = i.Quantity * i.Item.Price - itemDiscount;

                return (i.ServiceCharge.rate / 100m) * baseAmount;
            });

            // Total including tip
            dto.Total = dto.Subtotal - dto.DiscountTotal + dto.TaxTotal + dto.ServiceChargeTotal + (order.Tip ?? 0);
            //remaining totals after payments
            dto.AmountPaid = order.Payments
            .Where(p => p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount);

            dto.Remaining = dto.Total - dto.AmountPaid;
            dto.Remaining = Math.Max(0, dto.Remaining);
            dto.ChangeDue = Math.Max(0, dto.AmountPaid - dto.Total);
        }



    }
}
