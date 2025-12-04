using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;
using CentralizedSalesSystem.API.Models.Auth;

namespace CentralizedSalesSystem.API.Models.Orders.DTOs.ItemDTOs;



public class ItemReadDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public ItemType Type { get; set; }
    public int Stock { get; set; }
    public long BusinessId { get; set; }

    public List<ItemVariationReadDto> Variations { get; set; } = new();
    public List<Role> AssociatedRoles { get; set; } = new();


}
