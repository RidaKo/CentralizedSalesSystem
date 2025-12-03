using CentralizedSalesSystem.API.Models.Orders.enums;
namespace CentralizedSalesSystem.API.Models.Orders.DTOs.TaxDTOs;

public class TaxCreateDto
{
	public string Name { get; set; } = string.Empty;
	public decimal Rate { get; set; }
	public DateTimeOffset EffectiveFrom { get; set; }
	public DateTimeOffset? EffectiveTo { get; set; }
	public TaxStatus Status { get; set; }
	public long BusinessId { get; set; }
}