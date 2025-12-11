using System.Reflection;
using CentralizedSalesSystem.API.Models.Orders.enums;

namespace CentralizedSalesSystem.API.Models.Orders
{
    public class Payment
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset PaidAt { get; set; }

        public PaymentMethod Method { get; set; }
        public PaymentProvider Provider { get; set; }
        public PaymentCurrency Currency { get; set; }
        public PaymentStatus Status { get; set; }

        public long OrderId { get; set; }
        public Order? Order { get; set; }

        public long BussinesId { get; set; }



    }
}