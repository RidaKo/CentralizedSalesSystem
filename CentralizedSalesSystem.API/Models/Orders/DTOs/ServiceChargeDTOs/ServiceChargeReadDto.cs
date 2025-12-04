namespace CentralizedSalesSystem.API.Models.Orders.DTOs.ServiceChargeDTOs;

public class ServiceChargeReadDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long BusinessId { get; set; }
}