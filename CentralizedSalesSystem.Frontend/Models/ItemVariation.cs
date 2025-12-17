namespace CentralizedSalesSystem.Frontend.Models
{
    public class ItemVariation
    {
        public long Id { get; set; }
        public long ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Selection { get; set; } = "Required";
        
        public List<ItemVariationOption> Options { get; set; } = new();
    }
}