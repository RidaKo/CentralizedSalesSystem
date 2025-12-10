namespace CentralizedSalesSystem.Frontend.Models
{
    public class BusinessDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public string SubscriptionPlan { get; set; } = "catering";
    }
}