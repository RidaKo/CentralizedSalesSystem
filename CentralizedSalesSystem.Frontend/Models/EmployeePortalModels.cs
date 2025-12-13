using System;
using System.Collections.Generic;

namespace CentralizedSalesSystem.Frontend.Models
{
    public class TableDto
    {
        public long Id { get; set; }
        public long BusinessId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Status { get; set; } = "free"; // reserved | occupied | free
    }

    public class MenuItemDto
    {
        public long Id { get; set; }
        public long BusinessId { get; set; }
        public string Type { get; set; } = "item"; // item | service
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public List<string> Tags { get; set; } = new();
        public int? Stock { get; set; }
        public int? Duration { get; set; }
    }

    public class OrderItemDto
    {
        public long Id { get; set; }
        public long ItemId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public long? DiscountId { get; set; }
    }

    public class OrderDto
    {
        public long Id { get; set; }
        public long BusinessId { get; set; }
        public long? ReservationId { get; set; }
        public long? TableId { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public decimal Discount { get; set; }
        public decimal Tip { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? CreatedBy { get; set; }
        public string Status { get; set; } = "Open"; // Open | Closed | Paid | Refunded
        public List<PaymentDto> Payments { get; set; } = new();
    }

    public class PaymentDto
    {
        public long Id { get; set; }
        public string Method { get; set; } = "Cash"; // Cash | Card | Gift Card
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Completed"; // Placeholder for UI
        public DateTime? PaidAt { get; set; }
        public string Currency { get; set; } = "EUR";
    }
}
