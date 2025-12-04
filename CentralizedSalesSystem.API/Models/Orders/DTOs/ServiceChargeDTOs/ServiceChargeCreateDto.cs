namespace CentralizedSalesSystem.API.Models.Orders.DTOs.ServiceChargeDTOs;

public class ServiceChargeCreateDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public long BusinessId { get; set; }
}