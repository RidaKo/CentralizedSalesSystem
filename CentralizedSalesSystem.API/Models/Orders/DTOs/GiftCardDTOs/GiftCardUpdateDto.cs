using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;
using CentralizedSalesSystem.API.Models.Auth;

namespace CentralizedSalesSystem.API.Models.Orders.DTOs.GiftCardDTOs;

public class GiftCardUpdateDto
{
    public DateTimeOffset? ExpiresAt { get; set; }
    public GiftCardStatus? Status { get; set; }
    public decimal CurrentBalance { get; set; }

}
