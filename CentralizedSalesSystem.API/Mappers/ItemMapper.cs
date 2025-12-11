using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Auth;
namespace CentralizedSalesSystem.API.Mappers
{
    public static class ItemMapper
    {
        public static ItemResponseDto ToDto(this Item item)
        {
            return new ItemResponseDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Stock = item.Stock,
                Type = item.Type,
                BusinessId = item.BusinessId,
                Variations = item.Variations?.Select(v => new ItemVariationResponseDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    ItemId = v.ItemId,
                    Selection = v.Selection
                }).ToList() ?? new List<ItemVariationResponseDto>(),
                AssociatedRoles = item.Type == Models.Orders.enums.ItemType.Service && item.AssociatedRoles != null
                    ? item.AssociatedRoles.ToList()
                    : new List<Role>()//This has to be changed to RoleResponse
            };
        }
    }
}