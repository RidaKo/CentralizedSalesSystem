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
            // Use API's calculated subtotal if available, otherwise calculate locally
            if (order.Subtotal > 0)
                return order.Subtotal;
                
            return order.Items.Sum(line =>
            {
                var item = Items.FirstOrDefault(i => i.Id == line.ItemId);
                return (item?.Price ?? 0) * line.Quantity;
            });
        }

        private decimal GetDiscountAmount(OrderDto order)
        {
            // Use API's calculated discount total
            return order.DiscountTotal;
        }

        private decimal CalculateTotal(OrderDto order)
        {
            // Use API's calculated total if available
            if (order.Total > 0)
                return order.Total;
                
            var subtotal = GetSubtotal(order);
            var discount = GetDiscountAmount(order);
            var total = subtotal - discount + (order.Tip ?? 0);
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
