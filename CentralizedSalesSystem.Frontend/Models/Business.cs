namespace CentralizedSalesSystem.Frontend.Models
{
    public class Business
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SubscriptionPlan SubscriptionPlan { get; set; } = SubscriptionPlan.Catering;
        public string Address { get; set; } = string.Empty;
        public Currency Currency { get; set; } = Currency.EUR;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
