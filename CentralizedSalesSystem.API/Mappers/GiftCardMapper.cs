using CentralizedSalesSystem.API.Models.Orders.DTOs.GiftCardDTOs;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Auth;

namespace CentralizedSalesSystem.API.Mappers
{
    public static class GiftCardMapper
    {
        public static GiftCardResponseDto ToDto(this GiftCard giftCard)
        {
            return new GiftCardResponseDto
            {
                Id = giftCard.Id,
                Code = giftCard.Code,
                InitialValue = giftCard.InitialValue,
                CurrentBalance = giftCard.CurrentBalance,
                Currency = giftCard.Currency,
                IssuedAt = giftCard.IssuedAt,
                ExpiresAt = giftCard.ExpiresAt,
                IssuedBy = giftCard.IssuedBy,
                IssuedTo = giftCard.IssuedTo,
                Status = giftCard.Status,
                BusinessId = giftCard.BusinessId
            };
        }
    }
}
