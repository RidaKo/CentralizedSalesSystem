using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderDTOs;
using CentralizedSalesSystem.API.Models.Orders;

namespace CentralizedSalesSystem.API.Services
{
    public interface IOrderService
    {
        Task<object> GetOrdersAsync(
            int page,
            int limit,
            string? sortBy = null,
            string? sortDirection = null,
            string? filterByStatus = null,
            DateTimeOffset? filterByUpdatedAt = null,
            long? filterByBusinessId = null,
            long? filterByReservationId = null,
            long? filterByTableId = null);

        Task<OrderResponseDto?> GetOrderByIdAsync(long id);
        Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto dto);
        Task<OrderResponseDto?> UpdateOrderAsync(long id, OrderUpdateDto dto);
        Task<bool> DeleteOrderAsync(long id);
        void CalculateOrderTotals(Order order, OrderResponseDto dto);
    }
}

