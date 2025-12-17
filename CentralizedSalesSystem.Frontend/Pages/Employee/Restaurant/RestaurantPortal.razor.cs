using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CentralizedSalesSystem.Frontend.Models;
using CentralizedSalesSystem.Frontend.Json;
using MudBlazor;

namespace CentralizedSalesSystem.Frontend.Pages.Employee.Restaurant
{
    public partial class RestaurantPortal
    {
        private enum PortalView
        {
            CurrentOrder,
            Tables,
            AllOrders
        }

        private readonly long BusinessId = 1;

        private bool IsLoading = true;
        private PortalView ActiveView = PortalView.CurrentOrder;
        private string? SearchText;
        private string? SelectedCategory;
        private TableStatus? TableStatusFilter;
        private OrderStatus? OrderStatusFilter;
        private bool IsEditMode;
        private bool ShowPaymentsPanel;
        private bool ShowTipPanel;
        private bool ShowDiscountPanel;

        private decimal DiscountInput;
        private decimal TipInput;
        private decimal NewPaymentAmount;
        private PaymentMethod NewPaymentMethod = PaymentMethod.Card;
        private long? SelectedTableForNewOrder;
        private bool CanShowNewOrderButton => SelectedTableForNewOrder.HasValue && !HasOpenOrder(SelectedTableForNewOrder.Value);

        private List<TableDto> Tables { get; set; } = new();
        private List<MenuItemDto> Items { get; set; } = new();
        private List<OrderDto> Orders { get; set; } = new();

        private OrderDto? ActiveOrder { get; set; }
        private long? SelectedTableId { get; set; }

        private IEnumerable<MenuItemDto> FilteredMenuItems =>
            Items
                .Where(i => string.IsNullOrWhiteSpace(SelectedCategory) || i.Tags.Any(t => t.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase)))
                .Where(i => string.IsNullOrWhiteSpace(SearchText) || i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        private IEnumerable<string> Categories =>
            _defaultCategories.Concat(
                Items.SelectMany(i => i.Tags).Where(t => !string.IsNullOrWhiteSpace(t)))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        private IEnumerable<TableDto> FilteredTables =>
            Tables.Where(t => TableStatusFilter is null || t.Status == TableStatusFilter);

        private IEnumerable<OrderDto> FilteredOrders =>
            Orders.Where(o => OrderStatusFilter is null || o.Status == OrderStatusFilter);

        private decimal Subtotal => ActiveOrder is null
            ? 0
            : ActiveOrder.Items.Sum(line =>
            {
                var item = Items.FirstOrDefault(i => i.Id == line.ItemId);
                return (item?.Price ?? 0) * line.Quantity;
            });

        private decimal DiscountAmount => ActiveOrder is null ? 0 : GetDiscountAmount(ActiveOrder);
        private string DiscountPercentLabel => $"{(ActiveOrder?.Discount ?? 0):0.#}%";
        private decimal CurrentOrderTotal => ActiveOrder is null ? 0 : CalculateTotal(ActiveOrder);
        private decimal TotalPaid => ActiveOrder?.Payments.Sum(p => p.Amount) ?? 0;
        private decimal RemainingToPay => Math.Max(0, CurrentOrderTotal - TotalPaid);
        private decimal ChangeAmount => TotalPaid > CurrentOrderTotal ? TotalPaid - CurrentOrderTotal : 0;

        private string CurrentOrderTitle
        {
            get
            {
                var tableName = GetTableName(SelectedTableId);
                var guests = GetGuestCount();
                return !string.IsNullOrWhiteSpace(tableName)
                    ? $"{tableName} · {guests} guests"
                    : "No table selected";
            }
        }

        private readonly string[] _defaultCategories = new[]
        {
            "Appetizers", "Salads", "Soups", "Mains", "Fish", "Drinks", "Alcohol", "Desserts"
        };
    }
}
