using CentralizedSalesSystem.API.Models.Orders.enums;
namespace CentralizedSalesSystem.API.Models.Orders.DTOs.DiscountDTOs;

public class DiscountUpdateDto
{
    public string? Name { get; set; }
    public decimal? Rate { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
    public DiscountType? Type { get; set; }
    public DiscountAppliesTo? AppliesTo { get; set; }
    public DiscountStatus? Status { get; set; }
}