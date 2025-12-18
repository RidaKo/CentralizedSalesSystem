namespace CentralizedSalesSystem.API.Models.Orders.DTOs.OrderItemDTOs;

public class OrderItemCreateDto
{
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public long ItemId { get; set; }
    public long OrderId { get; set; }
    public long? ItemVariationOptionId { get; set; }
    public long? DiscountId { get; set; }
    public long? TaxId { get; set; }
    public long? ServiceChargeId { get; set; }
}
