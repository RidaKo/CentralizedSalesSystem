using CentralizedSalesSystem.API.Models.Orders.enums;

namespace CentralizedSalesSystem.API.Models.Orders

{
    public class ItemVariation
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public long ItemId { get; set; }

        public ItemVariationSelection Selection { get; set; }

        public Item Item { get; set; } = null!;

        public ICollection<ItemVariationOption> Options { get; set; } = new List<ItemVariationOption>();

    }
}
