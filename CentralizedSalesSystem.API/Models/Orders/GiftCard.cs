using CentralizedSalesSystem.API.Models.Orders.enums;

namespace CentralizedSalesSystem.API.Models.Orders
{
    public class GiftCard
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal InitialValue { get; set; }
        public decimal CurrentBalance { get; set; }
        public PaymentCurrency Currency { get; set; }
        public DateTimeOffset IssuedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public long IssuedBy { get; set; }
        public string? IssuedTo { get; set; }
        public GiftCardStatus Status { get; set; }
        public long BusinessId { get; set; }

        public ICollection<Payment> Redemptions { get; set; } = new List<Payment>();
    }
}
