using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderItemDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.PaymentDTOs;
using CentralizedSalesSystem.API.Models.Orders;
namespace CentralizedSalesSystem.API.Mappers
{
    public static class OrderMapper
    {
        public static OrderResponseDto ToOrderResponse(this Order order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                BusinessId = order.BusinessId,
                Tip = order.Tip,
                UpdatedAt = order.UpdatedAt,
                Status = order.Status,
                UserId = order.UserId,
                TableId = order.TableId,
                Discount = order.Discount != null ? order.Discount.ToDto() : null,
                ReservationId = order.ReservationId,
                Items = order.Items?.Select(i => i.ToDto()).ToList() ?? new List<OrderItemResponseDto>(),
                Payments = order.Payments?.Select(p => p.ToDto()).ToList() ?? new List<PaymentResponseDto>()

            };
        }
    }
}