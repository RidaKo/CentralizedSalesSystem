using CentralizedSalesSystem.API.Models.Orders.enums;

namespace CentralizedSalesSystem.API.Models.Orders.DTOs.RefundDTOs
{
    public class RefundUpdateDto
    {
        public decimal? Amount { get; set; }
        public string? Reason { get; set; }
        public DateTimeOffset? RefundedAt { get; set; }
        public PaymentMethod? RefundMethod { get; set; }
        public PaymentCurrency? Currency { get; set; }
        public PaymentStatus? Status { get; set; }
    }
}