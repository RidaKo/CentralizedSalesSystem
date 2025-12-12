using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Models.Reservations;
using System.Reflection;

namespace CentralizedSalesSystem.API.Models.Orders
{
	public class Order
	{
		public long Id { get; set; }
		public long BusinessId { get; set; }
		public decimal? Tip { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public OrderStatus Status { get; set; }

		public long UserId { get; set; }
		public long? TableId { get; set; }
		public long? DiscountId { get; set; }
		public long? ReservationId { get; set; }

		public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

		public User User { get; set; } = null!;
		public Table? Table { get; set; }
        public Discount? Discount { get; set; }
        public Reservation? Reservation { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();


    }
}