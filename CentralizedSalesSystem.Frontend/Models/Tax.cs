namespace CentralizedSalesSystem.Frontend.Models
{
    public class Tax
    {
        public long Id { get; set; }
        public long BusinessId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Rate { get; set; } // e.g. 0.21
        public string Status { get; set; } = "Active"; // enum in db, string in json
        public DateTimeOffset EffectiveFrom { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? EffectiveTo { get; set; }
    }
}