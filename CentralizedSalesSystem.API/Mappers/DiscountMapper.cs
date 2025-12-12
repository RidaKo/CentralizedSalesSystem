
using CentralizedSalesSystem.API.Models.Orders.DTOs.DiscountDTOs;
using CentralizedSalesSystem.API.Models.Orders;
namespace CentralizedSalesSystem.API.Mappers
{
    public static class DiscountMapper
    {
        public static DiscountResponseDto ToDto(this Discount discount)
        {
            return new DiscountResponseDto
            {
                Id = discount.Id,
                Name = discount.Name,
                Rate = discount.rate,
                ValidFrom = discount.ValidFrom,
                ValidTo = discount.ValidTo,
                Type = discount.Type,
                AppliesTo = discount.AppliesTo,
                Status = discount.Status,
                BusinessId = discount.BusinessId
            };
        }
    }
}