using CentralizedSalesSystem.API.Models.Orders.DTOs.OrderItemDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.DiscountDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.TaxDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ServiceChargeDTOs;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.enums;
namespace CentralizedSalesSystem.API.Mappers
{
    public static class OrderItemMapper
    {
        public static OrderItemResponseDto ToDto(this OrderItem oi)
        {
            return new OrderItemResponseDto
            {
                Id = oi.Id,
                Quantity = oi.Quantity,
                Notes = oi.Notes,
                ItemId = oi.ItemId,
                OrderId = oi.OrderId,
                DiscountId = oi.DiscountId,
                TaxId = oi.TaxId,
                ServiceChargeId = oi.ServiceChargeId,
                Item = oi.Item != null ? new ItemResponseDto
                {
                    Id = oi.Item.Id,
                    Name = oi.Item.Name,
                    Description = oi.Item.Description,
                    Price = oi.Item.Price,
                    Type = oi.Item.Type,
                    Stock = oi.Item.Stock,
                    BusinessId = oi.Item.BusinessId,
                    Variations = oi.Item.Variations?.Select(v => new ItemVariationResponseDto
                    {
                        Id = v.Id,
                        Name = v.Name,
                        ItemId = v.ItemId,
                        Selection = v.Selection
                    }).ToList() ?? new List<ItemVariationResponseDto>()
                } : null!,
                Discount = oi.Discount != null ? new DiscountResponseDto
                {
                    Id = oi.Discount.Id,
                    Name = oi.Discount.Name,
                    Rate = oi.Discount.rate,
                    ValidFrom = oi.Discount.ValidFrom,
                    ValidTo = oi.Discount.ValidTo,
                    Type = oi.Discount.Type,
                    AppliesTo = oi.Discount.AppliesTo,
                    Status = oi.Discount.Status,
                    BusinessId = oi.Discount.BusinessId
                } : null,
                Tax = oi.Tax != null && oi.Tax.Status == TaxStatus.Active &&
                      (oi.Tax.EffectiveTo == null || oi.Tax.EffectiveTo > DateTimeOffset.UtcNow) &&
                      oi.Tax.EffectiveFrom <= DateTimeOffset.UtcNow
                    ? new TaxResponseDto
                    {
                        Id = oi.Tax.Id,
                        Name = oi.Tax.Name,
                        Rate = oi.Tax.Rate,
                        BusinessId = oi.Tax.BusinessId
                    } : null,
                ServiceCharge = oi.ServiceCharge != null
                    ? new ServiceChargeResponseDto
                    {
                        Id = oi.ServiceCharge.Id,
                        Name = oi.ServiceCharge.Name,
                        Rate = oi.ServiceCharge.rate,
                        BusinessId = oi.ServiceCharge.BusinessId
                    } : null
            };
        }
    }
}