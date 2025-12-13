using CentralizedSalesSystem.API.Models.Orders.enums;
namespace CentralizedSalesSystem.API.Models.Orders.DTOs.PaymentDTOs;

public class PaymentCreateDto
{
    public DateTimeOffset PaidAt { get; set; }
    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; }
    public PaymentProvider Provider { get; set; }
    public PaymentCurrency Currency { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public long OrderId { get; set; }
    public long BussinesId { get; set; }

    public long? GiftCardId { get; set; }
}

