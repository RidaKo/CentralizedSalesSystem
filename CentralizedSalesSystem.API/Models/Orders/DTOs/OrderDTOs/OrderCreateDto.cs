using CentralizedSalesSystem.API.Models.Orders.enums;
namespace CentralizedSalesSystem.API.Models.Orders.DTOs.OrderDTOs;

public class OrderCreateDto
{
    public long BusinessId { get; set; }
    public decimal? Tip { get; set; }
    public OrderStatus Status { get; set; }
    public long UserId { get; set; }
    public long? TableId { get; set; }
    public long? DiscountId { get; set; }
    public long? ReservationId { get; set; }
    //public List<OrderItemCreateDto>? Items { get; set; }
}
