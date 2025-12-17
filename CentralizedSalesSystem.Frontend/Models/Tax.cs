namespace CentralizedSalesSystem.Frontend.Models
{
    public class Tax
    {
        public long Id { get; set; }
        public long BusinessId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        
        // changed from string to enum
        public TaxStatus Status { get; set; } = TaxStatus.Active; 
        
        public DateTimeOffset EffectiveFrom { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? EffectiveTo { get; set; }
    }

    public enum TaxStatus
    {
        Active,
        Inactive
    }
}