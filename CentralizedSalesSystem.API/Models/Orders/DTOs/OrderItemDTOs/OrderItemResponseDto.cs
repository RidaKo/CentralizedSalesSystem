using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.DiscountDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.TaxDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ServiceChargeDTOs;
using CentralizedSalesSystem.API.Models.Orders.DTOs.ItemVariationOptionDTOs;
namespace CentralizedSalesSystem.API.Models.Orders.DTOs.OrderItemDTOs;

public class OrderItemResponseDto
{
    public long Id { get; set; }
    public int Quantity { get; set; }
    public long? DiscountId { get; set; }
    public string? Notes { get; set; }

    public long ItemId { get; set; }
    public ItemResponseDto Item { get; set; } = null!;
    public long? ItemVariationOptionId { get; set; }
    public ItemVariationOptionResponseDto? ItemVariationOption { get; set; }

    public long OrderId { get; set; }
    public DiscountResponseDto? Discount { get; set; }

    public long? TaxId { get; set; }
    public TaxResponseDto? Tax { get; set; }

    public long? ServiceChargeId { get; set; }
    public ServiceChargeResponseDto? ServiceCharge { get; set; }
}

