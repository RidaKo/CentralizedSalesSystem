namespace CentralizedSalesSystem.Frontend.Models
{
    public class ItemVariationOption
    {
        public long Id { get; set; }
        public long VariationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PriceAdjustment { get; set; }
    }
}