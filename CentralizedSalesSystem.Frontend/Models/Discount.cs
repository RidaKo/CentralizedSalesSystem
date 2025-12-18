namespace CentralizedSalesSystem.Frontend.Models
{
    public class Discount
    {
        public long Id { get; set; }
        public long BusinessId { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public decimal Rate { get; set; }
        public DateTimeOffset ValidFrom { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ValidTo { get; set; }
        public DiscountType Type { get; set; } = DiscountType.Percentage; 
        public DiscountAppliesTo AppliesTo { get; set; } = DiscountAppliesTo.Order;
        public DiscountStatus Status { get; set; } = DiscountStatus.Active;
    }

    public enum DiscountType
    {
        Percentage,
        Fixed
    }

    public enum DiscountAppliesTo
    {
        Order,
        Product,
        Service
    }

    public enum DiscountStatus
    {
        Active,
        Inactive
    }
}
