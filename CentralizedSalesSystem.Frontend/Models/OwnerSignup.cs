using System.ComponentModel.DataAnnotations;

namespace CentralizedSalesSystem.Frontend.Models
{
    public sealed class OwnerSignupRequest
    {
        [Required]
        public OwnerSignupBusinessDto Business { get; set; } = new();

        [Required]
        public OwnerSignupOwnerDto Owner { get; set; } = new();
    }

    public sealed class OwnerSignupBusinessDto
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

    public sealed class OwnerSignupOwnerDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
    }

    public enum Currency
    {
        EUR,
        USD
    }

    public enum SubscriptionPlan
    {
        Catering,
        Beauty
    }

    public sealed class OwnerSignupResponse
    {
        public long BusinessId { get; set; }
        public long OwnerUserId { get; set; }
    }
}
