using CentralizedSalesSystem.API.Models.Orders.enums;
namespace CentralizedSalesSystem.API.Models.Orders.DTOs.PaymentDTOs;

public class PaymentUpdateDto
{
    public decimal? Amount { get; set; }
    public DateTimeOffset? PaidAt { get; set; }

    public PaymentMethod? Method { get; set; }
    public PaymentProvider? Provider { get; set; }
    public PaymentCurrency? Currency { get; set; }
    public PaymentStatus? Status { get; set; }

    public long? OrderId { get; set; }
    public long? BussinesId { get; set; }
}