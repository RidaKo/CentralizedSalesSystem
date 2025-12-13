using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;
using CentralizedSalesSystem.API.Models.Auth;

namespace CentralizedSalesSystem.API.Models.Orders.DTOs.GiftCardDTOs;



public class GiftCardResponseDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal InitialValue { get; set; }
    public decimal CurrentBalance { get; set; }
    public PaymentCurrency Currency { get; set; }
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public long IssuedBy { get; set; }
    public string? IssuedTo { get; set; }
    public GiftCardStatus Status { get; set; }
    public long BusinessId { get; set; }
}
