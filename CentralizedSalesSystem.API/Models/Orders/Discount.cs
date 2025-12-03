using CentralizedSalesSystem.API.Models.Orders.enums;
using System.Reflection;

namespace CentralizedSalesSystem.API.Models.Orders
{
    public class Discount
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal rate { get; set; }

        public DateTimeOffset ValidFrom { get; set; }
        public DateTimeOffset? ValidTo { get; set; }

        public DiscountType Type { get; set; }
        public DiscountAppliesTo AppliesTo { get; set; }
        public DiscountStatus Status { get; set; }

        public long BusinessId { get; set; }
    }
}