using CentralizedSalesSystem.API.Models;
using CentralizedSalesSystem.API.Models.Reservations;

namespace CentralizedSalesSystem.API.Models.Orders
{
    

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

        public ItemVariationOption? ItemVariationOption { get; set; }
        public long? ItemVariationOptionId { get; set; }

        public Reservation? Reservation { get; set; }
        public long? ReservationId { get; set; }

        public Order? Order { get; set; }
        public long OrderId { get; set; }
    }
}
