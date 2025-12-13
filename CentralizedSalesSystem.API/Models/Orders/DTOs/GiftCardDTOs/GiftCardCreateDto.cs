using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;
using CentralizedSalesSystem.API.Models.Auth;

namespace CentralizedSalesSystem.API.Models.Orders.DTOs.GiftCardDTOs;



public class GiftCardCreateDto
{
    public decimal InitialValue { get; set; }
    public PaymentCurrency Currency { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public long IssuedBy { get; set; }
    public string? IssuedTo { get; set; }
    public long BusinessId { get; set; }
}

