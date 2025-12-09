
namespace CentralizedSalesSystem.API.Models.Orders
{
    using CentralizedSalesSystem.API.Models;

    public class OrderItem
    {
        public long Id { get; set; }
        public int Quantity { get; set; }

        public string? Notes { get; set; }

        public long? TaxId { get; set; }
        public Tax? Tax { get; set; }

        public long? ServiceChargeId { get; set; }
        public ServiceCharge? ServiceCharge { get; set; }

        public Discount? Discount { get; set; }
        public long? DiscountId { get; set; }

        public Item? Item { get; set; }
        public long ItemId { get; set; }

        public Reservation? Reservation { get; set; }
        public long? ReservationId { get; set; }

        public Order? Order { get; set; }
        public long OrderId { get; set; }
    }
}