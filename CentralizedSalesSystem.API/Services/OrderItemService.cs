using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Mappers;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderItemDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderDTOs;

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

		public async Task<object> GetOrderItemsAsync(
		int page,
		int limit,
		string? sortBy = null,
		string? sortDirection = "asc",
		long? filterByOrderId = null,
		long? filterByItemId = null,
		long? filterByDiscountId = null
		)
		{
			var query = _db.OrderItems.AsQueryable();

			// FILTERING
			if (filterByOrderId.HasValue)
				query = query.Where(oi => oi.OrderId == filterByOrderId.Value);

			if (filterByItemId.HasValue)
				query = query.Where(oi => oi.ItemId == filterByItemId.Value);

			if (filterByDiscountId.HasValue)
				query = query.Where(oi => oi.DiscountId == filterByDiscountId.Value);

			// SORTING
			bool desc = sortDirection?.ToLower() == "desc";

			query = sortBy switch
			{
				"quantity" => desc ? query.OrderByDescending(oi => oi.Quantity) : query.OrderBy(oi => oi.Quantity),
				"id" => desc ? query.OrderByDescending(oi => oi.Id) : query.OrderBy(oi => oi.Id),
				_ => query
			};

			// PAGINATION
			var total = await query.CountAsync();
			var totalPages = (int)Math.Ceiling((double)total / limit);

			var items = await query.Skip((page - 1) * limit)
								   .Take(limit)
								   .ToListAsync();
			var result = items.Select(v => v.ToDto());
			
            return new
			{
				data = result,
				page,
				limit,
				total,
				totalPages
			};
		}


        public async Task<OrderItemResponseDto?> GetOrderItemByIdAsync(long orderItemId)
		{
			var oi = await _db.OrderItems
				.Include(oi => oi.Item)
					.ThenInclude(i => i.Variations)
				.Include(oi => oi.Discount)
				.Include(oi => oi.Taxes)
				.Include(oi => oi.ServiceCharges)
				.FirstOrDefaultAsync(oi => oi.Id == orderItemId);

			return oi == null ? null : oi.ToDto();
		}

		public async Task<OrderItemResponseDto?> CreateOrderItemAsync(OrderItemCreateDto dto)
		{
			var item = await _db.Items.FindAsync(dto.ItemId);
			var order = await _db.Orders.FindAsync(dto.OrderId); 

			if (item == null) return null;

			var orderItem = new OrderItem
			{
				OrderId = dto.OrderId,
				Item = item,
				Quantity = dto.Quantity,
				Notes = dto.Notes,
				DiscountId = dto.DiscountId
			};

			if (dto.TaxIds != null)
			{
				orderItem.Taxes = await _db.Taxes
					.Where(t => dto.TaxIds.Contains(t.Id))
					.ToListAsync();
			}

			if (dto.ServiceChargeIds != null)
			{
				orderItem.ServiceCharges = await _db.ServiceCharges
					.Where(sc => dto.ServiceChargeIds.Contains(sc.Id))
					.ToListAsync();
			}

			_db.OrderItems.Add(orderItem);
			await _db.SaveChangesAsync();

			return orderItem.ToDto();
		}

		public async Task<OrderItemResponseDto?> UpdateOrderItemAsync(long orderItemId, OrderItemUpdateDto dto)
		{
			var oi = await _db.OrderItems
				.Include(oi => oi.Taxes)
				.Include(oi => oi.ServiceCharges)
				.FirstOrDefaultAsync(oi => oi.Id == orderItemId);

			if (oi == null) return null;

			if (dto.Quantity.HasValue) oi.Quantity = dto.Quantity.Value;
			if (!string.IsNullOrWhiteSpace(dto.Notes)) oi.Notes = dto.Notes;
			if (dto.DiscountId.HasValue) oi.DiscountId = dto.DiscountId;

			if (dto.TaxIds != null)
			{
				oi.Taxes = await _db.Taxes
					.Where(t => dto.TaxIds.Contains(t.Id))
					.ToListAsync();
			}

			if (dto.ServiceChargeIds != null)
			{
				oi.ServiceCharges = await _db.ServiceCharges
					.Where(sc => dto.ServiceChargeIds.Contains(sc.Id))
					.ToListAsync();
			}

			await _db.SaveChangesAsync();
			return oi.ToDto();
		}

		public async Task<bool> DeleteOrderItemAsync(long orderItemId)
		{
			var oi = await _db.OrderItems.FindAsync(orderItemId);
			if (oi == null) return false;

			_db.OrderItems.Remove(oi);
			await _db.SaveChangesAsync();

			return true;
		}
	}
}
