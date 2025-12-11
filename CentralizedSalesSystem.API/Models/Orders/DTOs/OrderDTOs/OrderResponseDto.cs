using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderItemDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.DiscountDTOs;
namespace CentralizedSalesSystem.API.Models.Orders.DTOs.OrderDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.PaymentDTOs;


public class OrderResponseDto
{
    public long Id { get; set; }
    public long BusinessId { get; set; }
    public decimal? Tip { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public long UserId { get; set; }
    public long? TableId { get; set; }
    public long? DiscountId { get; set; }
    public long? ReservationId { get; set; }

    public DiscountResponseDto? Discount { get; set; }

    public ICollection<OrderItemResponseDto> Items { get; set; } = new List<OrderItemResponseDto>();

    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal ServiceChargeTotal { get; set; }
    public decimal Total { get; set; }

    public decimal AmountPaid { get; set; }
    public decimal Remaining { get; set; }
    public decimal ChangeDue { get; set; }

    public ICollection<PaymentResponseDto> Payments { get; set; } = new List<PaymentResponseDto>();


}