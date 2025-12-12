using CentralizedSalesSystem.API.Models.Business;

namespace CentralizedSalesSystem.API.Models.Business.DTOs;

public class BusinessResponseDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public long? Owner { get; set; }
    public Currency Currency { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; }
}
