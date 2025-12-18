using System;
using System.Linq;
using CentralizedSalesSystem.Frontend.Models;
using MudBlazor;

namespace CentralizedSalesSystem.Frontend.Pages.Employee.Restaurant
{
    public partial class RestaurantPortal
    {
        private string GetTableName(long? tableId)
        {
            if (tableId is null) return "—";
            return Tables.FirstOrDefault(t => t.Id == tableId)?.Name ?? $"Table {tableId}";
        }

        private string FormatPrice(OrderItemDto line, MenuItemDto? menuItem)
        {
            var price = menuItem?.Price ?? 0;
            return (price * line.Quantity).ToString("C");
        }

        private int GetGuestCount()
        {
            var table = Tables.FirstOrDefault(t => t.Id == SelectedTableId);
            if (table != null && table.Capacity > 0) return table.Capacity;
            if (ActiveOrder?.Items.Any() == true) return ActiveOrder.Items.Sum(i => i.Quantity);
            return 0;
        }

        private decimal GetSubtotal(OrderDto order)
        {
            return order.Items.Sum(line =>
            {
                var item = Items.FirstOrDefault(i => i.Id == line.ItemId);
                return (item?.Price ?? 0) * line.Quantity;
            });
        }

        private decimal GetDiscountAmount(OrderDto order)
        {
            if (order.Items == null || order.Items.Count == 0)
                return 0m;

            var subtotal = GetSubtotal(order);
            var itemDiscountTotal = order.Items.Sum(line =>
            {
                var item = Items.FirstOrDefault(i => i.Id == line.ItemId);
                if (item is null) return 0m;

                var discount = GetLineDiscount(line);
                if (discount is null) return 0m;
                if (!IsDiscountActive(discount)) return 0m;
                if (!IsDiscountApplicableToItem(discount, item)) return 0m;

                var lineTotal = item.Price * line.Quantity;
                return CalculateDiscountAmount(discount, lineTotal, line.Quantity);
            });

            var orderDiscount = GetOrderDiscount(order);
            if (orderDiscount is null || !IsDiscountActive(orderDiscount) || orderDiscount.AppliesTo != DiscountAppliesTo.Order)
            {
                return itemDiscountTotal;
            }

            var orderDiscountAmount = orderDiscount.Type == DiscountType.Percentage
                ? subtotal * (orderDiscount.Rate / 100m)
                : orderDiscount.Rate;
            orderDiscountAmount = Math.Min(orderDiscountAmount, subtotal);

            return itemDiscountTotal + orderDiscountAmount;
        }

        private string GetDiscountLabel(OrderDto order)
        {
            var orderDiscount = GetOrderDiscount(order);
            if (orderDiscount is not null && IsDiscountActive(orderDiscount) && orderDiscount.AppliesTo == DiscountAppliesTo.Order)
            {
                return orderDiscount.Name;
            }

            var hasItemDiscount = order.Items.Any(line =>
            {
                var item = Items.FirstOrDefault(i => i.Id == line.ItemId);
                if (item is null) return false;
                var discount = GetLineDiscount(line);
                return discount is not null && IsDiscountActive(discount) && IsDiscountApplicableToItem(discount, item);
            });

            return hasItemDiscount ? "Items" : "None";
        }

        private IEnumerable<Discount> GetOrderDiscountOptions() =>
            Discounts
                .Where(d => d.AppliesTo == DiscountAppliesTo.Order && IsDiscountActive(d))
                .OrderBy(d => d.Name);

        private IEnumerable<Discount> GetItemDiscountOptions(MenuItemDto? item)
        {
            if (item is null) return Enumerable.Empty<Discount>();

            var appliesTo = item.Type == ItemType.Service
                ? DiscountAppliesTo.Service
                : DiscountAppliesTo.Product;

            return Discounts
                .Where(d => d.AppliesTo == appliesTo && IsDiscountActive(d))
                .OrderBy(d => d.Name);
        }

        private string FormatDiscountOption(Discount discount) =>
            discount.Type == DiscountType.Percentage
                ? $"{discount.Name} ({discount.Rate:0.##}%)"
                : $"{discount.Name} ({discount.Rate.ToString("C")} off)";

        private Discount? GetOrderDiscount(OrderDto order) =>
            order.Discount ?? (order.DiscountId.HasValue ? Discounts.FirstOrDefault(d => d.Id == order.DiscountId.Value) : null);

        private Discount? GetLineDiscount(OrderItemDto line) =>
            line.Discount ?? (line.DiscountId.HasValue ? Discounts.FirstOrDefault(d => d.Id == line.DiscountId.Value) : null);

        private static bool IsDiscountApplicableToItem(Discount discount, MenuItemDto item) =>
            (discount.AppliesTo == DiscountAppliesTo.Product && item.Type == ItemType.Product)
            || (discount.AppliesTo == DiscountAppliesTo.Service && item.Type == ItemType.Service);

        private static bool IsDiscountActive(Discount discount)
        {
            if (discount.Status != DiscountStatus.Active) return false;

            var now = DateTimeOffset.UtcNow;
            if (discount.ValidFrom > now) return false;
            if (discount.ValidTo.HasValue && discount.ValidTo.Value < now) return false;

            return true;
        }

        private static decimal CalculateDiscountAmount(Discount discount, decimal baseAmount, int quantity)
        {
            var amount = discount.Type == DiscountType.Percentage
                ? baseAmount * (discount.Rate / 100m)
                : discount.Rate * quantity;

            return Math.Min(amount, baseAmount);
        }

        private decimal GetTaxAmount(OrderDto order)
        {
            if (order.Items == null || order.Items.Count == 0)
                return 0m;

            var now = DateTimeOffset.UtcNow;
            return order.Items.Sum(line =>
            {
                var item = Items.FirstOrDefault(i => i.Id == line.ItemId);
                if (item is null) return 0m;

                var tax = line.Tax
                    ?? (line.TaxId.HasValue ? Taxes.FirstOrDefault(t => t.Id == line.TaxId.Value) : null)
                    ?? (item.TaxId.HasValue ? Taxes.FirstOrDefault(t => t.Id == item.TaxId.Value) : null);
                if (tax == null) return 0m;
                if (tax.Status != TaxStatus.Active) return 0m;
                if (tax.EffectiveFrom > now) return 0m;
                if (tax.EffectiveTo.HasValue && tax.EffectiveTo.Value < now) return 0m;

                var lineSubtotal = item.Price * line.Quantity;
                return lineSubtotal * (tax.Rate / 100m);
            });
        }

        private decimal CalculateTotal(OrderDto order)
        {
            var subtotal = GetSubtotal(order);
            var discount = GetDiscountAmount(order);
            var tax = GetTaxAmount(order);
            var total = subtotal - discount + tax + order.ServiceChargeTotal + (order.Tip ?? 0);
            return Math.Max(total, 0);
        }

        private bool CanModifyActiveOrder =>
            ActiveOrder is not null && IsOpenStatus(ActiveOrder.Status);

        private static bool IsOpenStatus(OrderStatus status) =>
            status == OrderStatus.Open;

        private static bool CanRefund(OrderDto order) =>
            order.Status == OrderStatus.Closed &&
            order.Payments.Any(p => p.Status == PaymentStatus.Completed);

        private bool HasOpenOrder(long tableId) =>
            Orders.Any(o => o.TableId == tableId && IsOpenStatus(o.Status));

        private OrderDto? GetOrderForTable(long? tableId)
        {
            if (tableId is null) return null;

            var open = Orders
                .Where(o => o.TableId == tableId && IsOpenStatus(o.Status))
                .OrderByDescending(o => o.UpdatedAt ?? DateTime.MinValue)
                .FirstOrDefault();

            if (open is not null) return open;

            return Orders
                .Where(o => o.TableId == tableId)
                .OrderByDescending(o => o.UpdatedAt ?? DateTime.MinValue)
                .FirstOrDefault();
        }

        private void SetActiveOrderForTable(long? tableId)
        {
            SelectedTableId = tableId;
            SelectedTableForNewOrder = tableId;
            ActiveOrder = GetOrderForTable(tableId);
            SelectedOrderDiscountId = ActiveOrder?.DiscountId;
            IsEditMode = false;
        }

        private Color GetNavColor(PortalView view) => ActiveView == view ? Color.Success : Color.Default;

        private Color GetStatusColor(OrderStatus? status) =>
            status switch
            {
                OrderStatus.Open => Color.Warning,
                OrderStatus.Pending => Color.Info,
                OrderStatus.Closed => Color.Success,
                OrderStatus.Refunded => Color.Error,
                _ => Color.Primary
            };

        private long NextTempId() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        private void UpdateTableStatus(long? tableId, TableStatus status)
        {
            if (tableId is null) return;
            var table = Tables.FirstOrDefault(t => t.Id == tableId.Value);
            if (table != null)
            {
                table.Status = status;
            }
        }
    }
}
