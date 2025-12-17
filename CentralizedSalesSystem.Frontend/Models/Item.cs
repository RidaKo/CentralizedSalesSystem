namespace CentralizedSalesSystem.Frontend.Models
{
    public class Item
    {
        public long Id { get; set; }
        public long BusinessId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public ItemType Type { get; set; } = ItemType.Product;
        
        public int Stock { get; set; }
        
        public List<ItemVariation> Variations { get; set; } = new();
    }
}