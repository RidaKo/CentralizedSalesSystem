using System.Reflection;
using CentralizedSalesSystem.API.Models.Orders.enums;

namespace CentralizedSalesSystem.API.Models.Orders
{
    public class Refund
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset RefundedAt { get; set; }
        public string? Reason { get; set; }

        public PaymentMethod RefundMethod { get; set; }
        public PaymentCurrency Currency { get; set; }
        public PaymentStatus Status { get; set; }

        public long OrderId { get; set; }
        public Order? Order { get; set; }

        public long PaymentId { get; set; }
        public Payment? Payment { get; set; }



    }
}