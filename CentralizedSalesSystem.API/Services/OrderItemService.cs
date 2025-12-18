using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Mappers;
using CentralizedSalesSystem.API.Models;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderItemDTOs;
using CentralizedSalesSystem.API.Models.Orders.enums;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly CentralizedSalesDbContext _db;

        public OrderItemService(CentralizedSalesDbContext db)
        {
            _db = db;
        }

        public async Task<object> GetOrderItemsAsync(int page, int limit, string? sortBy = null, string? sortDirection = "asc",
            long? filterByOrderId = null, long? filterByItemId = null, long? filterByDiscountId = null)
        {
            var query = _db.OrderItems
                .Include(oi => oi.Item)
                    .ThenInclude(i => i.Variations)
                        .ThenInclude(v => v.Options)
                .Include(oi => oi.Discount)
                .Include(oi => oi.Tax)
                .Include(oi => oi.ServiceCharge)
                .Include(oi => oi.ItemVariationOption)
                    .ThenInclude(o => o.ItemVariation)
                .AsQueryable();

            if (filterByOrderId.HasValue) query = query.Where(oi => oi.OrderId == filterByOrderId.Value);
            if (filterByItemId.HasValue) query = query.Where(oi => oi.ItemId == filterByItemId.Value);
            if (filterByDiscountId.HasValue) query = query.Where(oi => oi.DiscountId == filterByDiscountId.Value);

            bool desc = sortDirection?.ToLower() == "desc";
            query = sortBy switch
            {
                "quantity" => desc ? query.OrderByDescending(oi => oi.Quantity) : query.OrderBy(oi => oi.Quantity),
                _ => query
            };

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            var result = items.Select(i => i.ToDto()).ToList();

            return new { data = result, page, limit, total, totalPages = (int)Math.Ceiling(total / (double)limit) };
        }

        public async Task<OrderItemResponseDto?> GetOrderItemByIdAsync(long id)
        {
            var item = await _db.OrderItems
                .Include(oi => oi.Item)
                    .ThenInclude(i => i.Variations)
                        .ThenInclude(v => v.Options)
                .Include(oi => oi.Discount)
                .Include(oi => oi.Tax)
                .Include(oi => oi.ServiceCharge)
                .Include(oi => oi.ItemVariationOption)
                    .ThenInclude(o => o.ItemVariation)
                .FirstOrDefaultAsync(oi => oi.Id == id);

            return item?.ToDto();
        }

        public async Task<OrderItemResponseDto?> CreateOrderItemAsync(OrderItemCreateDto dto)
        {
            var order = await _db.Orders.FindAsync(dto.OrderId);
            if (order == null)
                throw new Exception($"Order {dto.OrderId} not found");

            var item = await _db.Items.FindAsync(dto.ItemId);
            if (item == null)
                throw new Exception($"Item {dto.ItemId} not found");

            ItemVariationOption? variationOption = null;
            if (dto.ItemVariationOptionId.HasValue && dto.ItemVariationOptionId.Value > 0)
            {
                variationOption = await _db.ItemVariationOptions
                    .Include(o => o.ItemVariation)
                    .FirstOrDefaultAsync(o => o.Id == dto.ItemVariationOptionId.Value);

                if (variationOption == null || variationOption.ItemVariation.ItemId != dto.ItemId)
                    throw new Exception("Invalid variation option for this item");
            }
            else
            {
                var hasRequiredVariation = await _db.ItemVariations
                    .AnyAsync(v => v.ItemId == dto.ItemId && v.Selection == ItemVariationSelection.Required);

                if (hasRequiredVariation)
                    throw new Exception($"A variation selection is required for {item.Name}");
            }

            if (item.Stock < dto.Quantity)
                throw new Exception($"Insufficient stock for item {item.Name}");

            item.Stock -= dto.Quantity;

            var orderItem = new OrderItem
            {
                OrderId = dto.OrderId,
                ItemId = dto.ItemId,
                Quantity = dto.Quantity,
                Notes = dto.Notes,
                ItemVariationOptionId = variationOption?.Id,
                ItemVariationOption = variationOption,
                DiscountId = dto.DiscountId,
                TaxId = dto.TaxId ?? item.TaxId,
                ServiceChargeId = dto.ServiceChargeId
            };

            _db.OrderItems.Add(orderItem);
            await _db.SaveChangesAsync();

            return await GetOrderItemByIdAsync(orderItem.Id);
        }


        public async Task<OrderItemResponseDto?> UpdateOrderItemAsync(long id, OrderItemUpdateDto dto)
        {
            var orderItem = await _db.OrderItems.FindAsync(id);
            if (orderItem == null) return null;

            if (dto.Quantity.HasValue)
            {
                var item = await _db.Items.FindAsync(orderItem.ItemId);
                if (item == null) throw new Exception($"Item {orderItem.ItemId} not found");

                int delta = dto.Quantity.Value - orderItem.Quantity;
                if (item.Stock < delta) throw new Exception($"Insufficient stock for item {item.Name}");

                item.Stock -= delta;
                orderItem.Quantity = dto.Quantity.Value;
            }

            if (!string.IsNullOrEmpty(dto.Notes)) orderItem.Notes = dto.Notes;
            if (dto.ItemVariationOptionId.HasValue)
            {
                if (dto.ItemVariationOptionId.Value == 0)
                {
                    var hasRequiredVariation = await _db.ItemVariations
                        .AnyAsync(v => v.ItemId == orderItem.ItemId && v.Selection == ItemVariationSelection.Required);

                    if (hasRequiredVariation)
                        throw new Exception("A variation selection is required for this item");

                    orderItem.ItemVariationOptionId = null;
                    orderItem.ItemVariationOption = null;
                }
                else
                {
                    var variationOption = await _db.ItemVariationOptions
                        .Include(o => o.ItemVariation)
                        .FirstOrDefaultAsync(o => o.Id == dto.ItemVariationOptionId.Value);

                    if (variationOption == null || variationOption.ItemVariation.ItemId != orderItem.ItemId)
                        throw new Exception("Invalid variation option for this item");

                    orderItem.ItemVariationOptionId = variationOption.Id;
                    orderItem.ItemVariationOption = variationOption;
                }
            }
            if (dto.DiscountId.HasValue)
            {
                orderItem.DiscountId = dto.DiscountId.Value == 0 ? null : dto.DiscountId;
            }
            if (dto.TaxId.HasValue) orderItem.TaxId = dto.TaxId;
            if (dto.ServiceChargeId.HasValue) orderItem.ServiceChargeId = dto.ServiceChargeId;

            await _db.SaveChangesAsync();
            return await GetOrderItemByIdAsync(orderItem.Id);
        }

        public async Task<bool> DeleteOrderItemAsync(long id)
        {
            var orderItem = await _db.OrderItems.FindAsync(id);
            if (orderItem == null) return false;

            var item = await _db.Items.FindAsync(orderItem.ItemId);
            if (item != null) item.Stock += orderItem.Quantity; // restore stock

            _db.OrderItems.Remove(orderItem);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
