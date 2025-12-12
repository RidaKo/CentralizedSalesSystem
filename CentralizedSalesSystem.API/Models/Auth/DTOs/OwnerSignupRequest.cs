using System.ComponentModel.DataAnnotations;
using CentralizedSalesSystem.API.Models.Business;

namespace CentralizedSalesSystem.API.Models.Auth.DTOs;

public class OwnerSignupRequest
{
    [Required]
    public OwnerSignupBusinessDto Business { get; set; } = new();

    [Required]
    public OwnerSignupOwnerDto Owner { get; set; } = new();
}

public class OwnerSignupBusinessDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Phone { get; set; } = string.Empty;
    [Required]
    public string Address { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    public string Country { get; set; } = "N/A";
    public Currency Currency { get; set; } = Currency.EUR;
    public SubscriptionPlan SubscriptionPlan { get; set; } = SubscriptionPlan.Catering;
}

public class OwnerSignupOwnerDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
