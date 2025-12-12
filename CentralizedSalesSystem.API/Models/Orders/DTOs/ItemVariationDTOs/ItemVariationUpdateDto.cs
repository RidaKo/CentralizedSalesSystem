using CentralizedSalesSystem.API.Models.Orders.enums;
namespace CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;

public class ItemVariationUpdateDto
{
    public string? Name { get; set; }
    public long? ItemId { get; set; }
    public ItemVariationSelection? Selection { get; set; }
}