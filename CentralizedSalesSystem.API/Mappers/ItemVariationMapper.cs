using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationOptionDTOs;
using CentralizedSalesSystem.API.Models.Orders;
namespace CentralizedSalesSystem.API.Mappers
{
    public static class ItemVariationMapper
    {
        public static ItemVariationResponseDto ToDto(this ItemVariation variation)
        {
            return new ItemVariationResponseDto
            {
                Id = variation.Id,
                Name = variation.Name,
                ItemId = variation.ItemId,
                Selection = variation.Selection,
                Options = variation.Options?
                    .Select(o => new ItemVariationOptionResponseDto
                    {
                        Id = o.Id,
                        Name = o.Name,
                        PriceAdjustment = o.PriceAdjustment,
                        ItemVariationId = o.ItemVariationId
                    }).ToList() ?? new List<ItemVariationOptionResponseDto>()
            };
        }
    }
}
