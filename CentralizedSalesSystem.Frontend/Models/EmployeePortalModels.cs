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
        public TableStatus Status { get; set; } = TableStatus.Free;
    }

    public class MenuItemDto
    {
        public long Id { get; set; }
        public long BusinessId { get; set; }
        public ItemType Type { get; set; } = ItemType.Product;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int? Stock { get; set; }
        public int? Duration { get; set; }
        public long? TaxId { get; set; }
    }

    public class OrderItemDto
    {
        public long Id { get; set; }
        public long ItemId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public long? DiscountId { get; set; }
        public Discount? Discount { get; set; }
        public long? TaxId { get; set; }
        public Tax? Tax { get; set; }
    }

    public class OrderDto
    {
        public long Id { get; set; }
        public long BusinessId { get; set; }
        public long UserId { get; set; }
        public long? ReservationId { get; set; }
        public long? TableId { get; set; }
        public long? DiscountId { get; set; }
        public Discount? Discount { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public decimal? Tip { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public long? CreatedBy { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Open;
        public List<PaymentDto> Payments { get; set; } = new();
        
        // Totals from API
        public decimal Subtotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal ServiceChargeTotal { get; set; }
        public decimal Total { get; set; }
        public decimal AmountPaid { get; set; }
    }

    public class PaymentDto
    {
        public long Id { get; set; }
        public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
        public DateTimeOffset? PaidAt { get; set; }
        public PaymentCurrency Currency { get; set; } = PaymentCurrency.EUR;
    }

    public enum TableStatus
    {
        Reserved,
        Occupied,
        Free,
    }

    public enum OrderStatus
    {
        Open,
        Pending,
        Closed,
        Refunded,
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded,
    }

    public enum PaymentMethod
    {
        Cash,
        Card,
        GiftCard,
    }

    public enum PaymentProvider
    {
        Internal,
        Stripe,
        PayPal,
    }

    public enum PaymentCurrency
    {
        EUR,
        USD,
    }

    public enum ItemType
    {
        Product,
        Service,
    }
}
