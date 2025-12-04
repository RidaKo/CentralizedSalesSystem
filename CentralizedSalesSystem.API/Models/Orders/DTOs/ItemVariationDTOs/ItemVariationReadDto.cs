using CentralizedSalesSystem.API.Models.Orders.enums;
namespace CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationDTOs;

public class ItemVariationReadDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long ItemId { get; set; }
    public ItemVariationSelection Selection { get; set; }
}

