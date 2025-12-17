namespace CentralizedSalesSystem.Frontend.Models
{
    public class Discount
    {
        public long Id { get; set; }
        public long BusinessId { get; set; }
        public string Name { get; set; } = string.Empty;
        
        // changed from string to enum
        public DiscountType Type { get; set; } = DiscountType.Percentage; 
        
        public decimal Rate { get; set; }
        public DateTimeOffset ValidFrom { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ValidTo { get; set; }
        public string Status { get; set; } = "Active";
    }

    public enum DiscountType
    {
        Percentage,
        Fixed
    }
}