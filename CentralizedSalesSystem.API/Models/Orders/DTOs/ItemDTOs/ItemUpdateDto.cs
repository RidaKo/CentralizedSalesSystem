using CentralizedSalesSystem.API.Models.Orders.enums;
namespace CentralizedSalesSystem.API.Models.Orders.DTOs.ItemDTOs;

public class ItemUpdateDto
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public string? Description { get; set; }
    public int? Stock { get; set; }
    public int? Duration { get; set; }
    public ItemType? Type { get; set; }
    public long? BusinessId { get; set; }

}
