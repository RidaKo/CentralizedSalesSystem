using CentralizedSalesSystem.API.Models.Business;

namespace CentralizedSalesSystem.API.Models.Business.DTOs;

public class BusinessCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public long Owner { get; set; }
    public Currency Currency { get; set; } = Currency.EUR;
    public SubscriptionPlan SubscriptionPlan { get; set; } = SubscriptionPlan.Catering;
}
