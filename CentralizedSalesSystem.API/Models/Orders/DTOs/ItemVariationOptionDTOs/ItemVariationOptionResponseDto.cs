namespace CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationOptionDTOs;

public class ItemVariationOptionResponseDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceAdjustment { get; set; }
    public long ItemVariationId { get; set; }
}
