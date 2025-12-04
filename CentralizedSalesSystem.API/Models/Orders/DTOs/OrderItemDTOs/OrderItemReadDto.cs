using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.DiscountDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.TaxDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ServiceChargeDTOs;

namespace CentralizedSalesSystem.API.Models.Orders.DTOs.OrderItemDTOs;

public class OrderItemReadDto
{
    public long Id { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public long ItemId { get; set; }
    public long? DiscountId { get; set; }

    public ItemReadDto Item { get; set; } = null!;
    public DiscountReadDto? Discount { get; set; }
    public List<TaxReadDto> Taxes { get; set; } = new();
    public List<ServiceChargeReadDto> ServiceCharges { get; set; } = new();
}