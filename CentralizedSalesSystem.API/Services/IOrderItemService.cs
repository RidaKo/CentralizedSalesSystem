using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderItemDTOs;

namespace CentralizedSalesSystem.API.Services
{
	public interface IOrderItemService
	{
		Task<object> GetOrderItemsAsync(
			int page,
			int limit,
			string? sortBy = null,
			string? sortDirection = null,
			long? filterByItemId = null,
			long? filterByDiscountId = null);

		Task<OrderItemResponseDto?> GetOrderItemByIdAsync(long id);
		Task<OrderItemResponseDto?> CreateOrderItemAsync(OrderItemCreateDto dto);
		Task<OrderItemResponseDto?> UpdateOrderItemAsync(long id, OrderItemUpdateDto dto);
		Task<bool> DeleteOrderItemAsync(long id);
	}
}