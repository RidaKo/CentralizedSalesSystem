using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Models.Auth;
namespace CentralizedSalesSystem.API.Models.Orders

{
    public class Item
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public long BusinessId { get; set; }

        public ItemType Type { get; set; }

        public long? TaxId { get; set; }
        public Tax? Tax { get; set; }

        public ICollection<ItemVariation> Variations { get; set; } = new List<ItemVariation>();
        public ICollection<Role> AssociatedRoles { get; set; } = new List<Role>();


        public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
    }
}
