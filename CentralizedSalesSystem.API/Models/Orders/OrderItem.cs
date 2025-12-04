
namespace CentralizedSalesSystem.API.Models.Orders
{
    public class OrderItem
    {
        public long Id { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }

        public long ItemId { get; set; }
        public long? DiscountId { get; set; }

        public ICollection<Tax> Taxes { get; set; } = new List<Tax>();
        public ICollection<ServiceCharge>? ServiceCharge { get; set; } = new List<ServiceCharge>();


        public Item Item { get; set; } = null!;
        public Discount? Discount { get; set; }

    }
}