namespace CentralizedSalesSystem.API.Models.Orders.DTOs.ServiceChargeDTOs;

public class ServiceChargeResponseDto
{
    public long Id { get; set; }
    public long BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
