using CentralizedSalesSystem.API.Models.Orders.enums;
namespace CentralizedSalesSystem.API.Models.Orders.DTOs.TaxDTOs;

public class TaxUpdateDto
{
    public string? Name { get; set; }
    public decimal? Rate { get; set; }
    public DateTimeOffset? EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public TaxStatus? Status { get; set; }
    public long? BusinessId { get; set; }
}