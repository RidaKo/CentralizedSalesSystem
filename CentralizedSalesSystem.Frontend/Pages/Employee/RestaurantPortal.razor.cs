using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CentralizedSalesSystem.Frontend.Models;
using CentralizedSalesSystem.Frontend.Json;
using MudBlazor;

namespace CentralizedSalesSystem.Frontend.Pages.Employee
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

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                await Task.WhenAll(LoadTablesAsync(), LoadItemsAsync(), LoadOrdersAsync());
                SyncTableStatuses();
                SelectedCategory ??= Categories.FirstOrDefault() ?? "Mains";
                EnsureSelections();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadTablesAsync()
        {
            try
            {
                var response = await Http.GetFromJsonAsync<PaginatedResponse<TableDto>>($"tables?limit=200&filterByBusinessId={BusinessId}");
                Tables = response?.Data ?? new List<TableDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load tables: {ex.Message}. Using mock data.");
                LoadMockTables();
                Snackbar.Add("Using mock tables", Severity.Info);
            }
        }

        private async Task LoadItemsAsync()
        {
            try
            {
                var response = await Http.GetFromJsonAsync<PaginatedResponse<MenuItemDto>>($"items?limit=200&filterByBusinessId={BusinessId}");
                Items = response?.Data ?? new List<MenuItemDto>();
                if (Items.Count == 0)
                {
                    LoadMockItems();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load items: {ex.Message}. Using mock data.");
                LoadMockItems();
                Snackbar.Add("Using mock menu", Severity.Info);
            }
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                var response = await Http.GetFromJsonAsync<PaginatedResponse<OrderDto>>($"orders?limit=100&filterByBusinessId={BusinessId}");
                Orders = response?.Data ?? new List<OrderDto>();
                if (Orders.Count == 0)
                {
                    LoadMockOrders();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load orders: {ex.Message}. Using mock data.");
                LoadMockOrders();
                Snackbar.Add("Using mock orders", Severity.Info);
            }
        }

        private void EnsureSelections()
        {
            var initialTable = ActiveOrder?.TableId ?? Tables.FirstOrDefault()?.Id;
            SetActiveOrderForTable(initialTable);
            ActiveOrder ??= Orders.FirstOrDefault(o => IsOpenStatus(o.Status)) ?? Orders.FirstOrDefault();
            ActiveView = PortalView.CurrentOrder;
        }

        private void CreateOrderFromSelection()
        {
            if (!SelectedTableForNewOrder.HasValue)
            {
                Snackbar.Add("Select a table to create an order.", Severity.Warning);
                return;
            }

            var tableId = SelectedTableForNewOrder.Value;
            if (HasOpenOrder(tableId))
            {
                Snackbar.Add("This table already has an open order.", Severity.Info);
                return;
            }

            var table = Tables.FirstOrDefault(t => t.Id == tableId);
            ActiveOrder = new OrderDto
            {
                Id = NextTempId(),
                BusinessId = BusinessId,
                TableId = tableId,
                Status = OrderStatus.Open,
                UpdatedAt = DateTime.UtcNow
            };
            Orders.Insert(0, ActiveOrder);
            SelectedTableId = tableId;
            UpdateTableStatus(tableId, TableStatus.Occupied);
            ActiveView = PortalView.CurrentOrder;
            IsEditMode = true;
            Snackbar.Add($"Created new order for {table?.Name ?? $"Table {tableId}"}", Severity.Success);
        }

        private void AddItemToOrder(MenuItemDto menuItem)
        {
            if (!CanModifyActiveOrder)
            {
                Snackbar.Add("Order is locked. Select an open order to modify.", Severity.Warning);
                return;
            }

            if (ActiveOrder is null)
            {
                Snackbar.Add("Create or select an order first.", Severity.Warning);
                return;
            }

            var existingLine = ActiveOrder.Items.FirstOrDefault(i => i.ItemId == menuItem.Id);
            if (existingLine is null)
            {
                ActiveOrder.Items.Add(new OrderItemDto
                {
                    Id = NextTempId(),
                    ItemId = menuItem.Id,
                    Quantity = 1
                });
            }
            else
            {
                if (existingLine.Quantity >= 2 && !IsEditMode)
                {
                    Snackbar.Add("Reached quick-add limit (2). Use Edit to change quantity.", Severity.Info);
                    return;
                }
                existingLine.Quantity += 1;
            }

            ActiveOrder.UpdatedAt = DateTime.UtcNow;
        }

        private void ChangeQuantity(OrderItemDto line, int delta)
        {
            if (!CanModifyActiveOrder || ActiveOrder is null) return;
            line.Quantity = Math.Clamp(line.Quantity + delta, 1, 10);
            ActiveOrder.UpdatedAt = DateTime.UtcNow;
        }

        private void RemoveItem(OrderItemDto line)
        {
            if (!CanModifyActiveOrder || ActiveOrder is null) return;
            ActiveOrder.Items.Remove(line);
            ActiveOrder.UpdatedAt = DateTime.UtcNow;
        }

        private void SelectCategory(string category)
        {
            SelectedCategory = category;
        }

        private void SelectTable(TableDto table)
        {
            SetActiveOrderForTable(table.Id);
            ActiveView = PortalView.CurrentOrder;
        }

        private Task OnTableSelectionChanged(long? tableId)
        {
            SetActiveOrderForTable(tableId);
            return Task.CompletedTask;
        }

        private void SwitchView(PortalView view) => ActiveView = view;

        private void OnOrderRowClick(TableRowClickEventArgs<OrderDto> args)
        {
            if (args.Item is null) return;

            ActiveOrder = args.Item;
            SelectedTableId = args.Item.TableId;
            SelectedTableForNewOrder = args.Item.TableId;
            ActiveView = PortalView.CurrentOrder;
        }

        private void ToggleEdit()
        {
            if (!CanModifyActiveOrder) return;
            IsEditMode = !IsEditMode;
        }

        private void TogglePayments() => ShowPaymentsPanel = !ShowPaymentsPanel;
        private void ToggleTip() => ShowTipPanel = !ShowTipPanel;
        private void ToggleDiscount() => ShowDiscountPanel = !ShowDiscountPanel;

        private void SendOrder()
        {
            if (ActiveOrder is null) return;
            ActiveOrder.UpdatedAt = DateTime.UtcNow;
            Snackbar.Add("Order sent to the kitchen (mock).", Severity.Success);
        }

        private void CancelOrder()
        {
            if (ActiveOrder is null || !CanModifyActiveOrder) return;
            ActiveOrder.Status = OrderStatus.Closed;
            ActiveOrder.UpdatedAt = DateTime.UtcNow;
            UpdateTableStatus(ActiveOrder.TableId, TableStatus.Free);
            SyncTableStatuses();
            IsEditMode = false;
            Snackbar.Add("Order cancelled.", Severity.Warning);
        }

        private void ApplyDiscountAmount()
        {
            if (ActiveOrder is null || !CanModifyActiveOrder) return;
            ActiveOrder.Discount = Math.Clamp(DiscountInput, 0, 100);
            ActiveOrder.UpdatedAt = DateTime.UtcNow;
            Snackbar.Add("Discount applied.", Severity.Success);
        }

        private void ApplyTipPercent(decimal percent)
        {
            if (ActiveOrder is null || !CanModifyActiveOrder) return;
            ActiveOrder.Tip = Math.Round(Subtotal * percent, 2);
            ActiveOrder.UpdatedAt = DateTime.UtcNow;
        }

        private void ApplyCustomTip()
        {
            if (ActiveOrder is null || !CanModifyActiveOrder) return;
            ActiveOrder.Tip = Math.Max(0, TipInput);
            ActiveOrder.UpdatedAt = DateTime.UtcNow;
        }

        private void AddPayment()
        {
            if (ActiveOrder is null || !CanModifyActiveOrder) return;

            var amount = NewPaymentAmount > 0 ? NewPaymentAmount : RemainingToPay;
            if (amount <= 0) return;

            ActiveOrder.Payments.Add(new PaymentDto
            {
                Id = NextTempId(),
                Method = NewPaymentMethod,
                Amount = amount,
                PaidAt = DateTime.UtcNow,
                Status = PaymentStatus.Completed
            });

            ActiveOrder.UpdatedAt = DateTime.UtcNow;
            NewPaymentAmount = 0;

            if (TotalPaid >= CurrentOrderTotal)
            {
                ActiveOrder.Status = OrderStatus.Closed;
                UpdateTableStatus(ActiveOrder.TableId, TableStatus.Free);
                SyncTableStatuses();
                IsEditMode = false;
            }
        }

        private void PayOrder()
        {
            if (ActiveOrder is null || RemainingToPay <= 0 || !CanModifyActiveOrder) return;
            NewPaymentAmount = RemainingToPay;
            AddPayment();
            Snackbar.Add("Order marked as paid (mock).", Severity.Success);
        }

        private async Task ShowReceipt(OrderDto? order = null)
        {
            var target = order ?? ActiveOrder;
            if (target is null) return;

            var lines = target.Items.Select(line =>
            {
                var item = Items.FirstOrDefault(i => i.Id == line.ItemId);
                var name = item?.Name ?? "Item";
                var price = (item?.Price ?? 0) * line.Quantity;
                return $"{name} x{line.Quantity} - {price:C}";
            });

            var payments = target.Payments.Any()
                ? string.Join("\n", target.Payments.Select(p => $"{p.Method}: {p.Amount:C}"))
                : "No payments.";

            var subtotal = GetSubtotal(target);
            var discount = GetDiscountAmount(target);
            var total = CalculateTotal(target);

            var text = string.Join("\n", lines)
                       + $"\nSubtotal: {subtotal:C}"
                       + $"\nDiscount ({target.Discount}%): {discount:C}"
                       + $"\nTip: {target.Tip:C}"
                       + $"\nTotal: {total:C}"
                       + $"\n\nPayments:\n{payments}";

            await DialogService.ShowMessageBox("Receipt", text, yesText: "Close");
        }

        private void RefundOrder(OrderDto? order = null)
        {
            var target = order ?? ActiveOrder;
            if (target is null || !CanRefund(target)) return;
            target.Status = OrderStatus.Refunded;
            target.UpdatedAt = DateTime.UtcNow;
            UpdateTableStatus(target.TableId, TableStatus.Free);
            SyncTableStatuses();
            Snackbar.Add("Order marked as refunded (mock).", Severity.Warning);
        }

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

        private decimal GetSubtotal(OrderDto order) =>
            order.Items.Sum(line =>
            {
                var item = Items.FirstOrDefault(i => i.Id == line.ItemId);
                return (item?.Price ?? 0) * line.Quantity;
            });

        private decimal GetDiscountAmount(OrderDto order)
        {
            var subtotal = GetSubtotal(order);
            return order.Discount > 0 ? Math.Round(subtotal * order.Discount / 100m, 2) : 0;
        }

        private decimal CalculateTotal(OrderDto order)
        {
            var subtotal = GetSubtotal(order);
            var discount = GetDiscountAmount(order);
            var total = subtotal - discount + order.Tip;
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

        private Color GetTableColor(TableStatus status) => status switch
        {
            TableStatus.Occupied => Color.Success,
            TableStatus.Reserved => Color.Warning,
            _ => Color.Default
        };

        private string GetTableCardClass(TableDto table)
        {
            var status = table.Status.ToString().ToLowerInvariant();
            var isSelected = table.Id == SelectedTableId;
            return $"table-card {status}" + (isSelected ? " selected" : string.Empty);
        }

        private Color GetCategoryColor(string category) =>
            string.Equals(category, SelectedCategory, StringComparison.OrdinalIgnoreCase) ? Color.Success : Color.Default;

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

        private void ToggleReservation(TableDto table)
        {
            if (table.Status == TableStatus.Occupied)
            {
                Snackbar.Add("Cannot reserve an occupied table.", Severity.Warning);
                return;
            }

            table.Status = table.Status == TableStatus.Reserved
                ? TableStatus.Free
                : TableStatus.Reserved;
        }

        private void LoadMockTables()
        {
            Tables = new List<TableDto>
            {
                new() { Id = 1, BusinessId = BusinessId, Name = "Table 1", Capacity = 2, Status = TableStatus.Free },
                new() { Id = 2, BusinessId = BusinessId, Name = "Table 2", Capacity = 2, Status = TableStatus.Free },
                new() { Id = 3, BusinessId = BusinessId, Name = "Table 3", Capacity = 2, Status = TableStatus.Free },
                new() { Id = 4, BusinessId = BusinessId, Name = "Table 4", Capacity = 4, Status = TableStatus.Reserved },
                new() { Id = 5, BusinessId = BusinessId, Name = "Table 5", Capacity = 4, Status = TableStatus.Free },
                new() { Id = 6, BusinessId = BusinessId, Name = "Table 6", Capacity = 4, Status = TableStatus.Free },
                new() { Id = 7, BusinessId = BusinessId, Name = "Table 7", Capacity = 6, Status = TableStatus.Reserved },
                new() { Id = 8, BusinessId = BusinessId, Name = "Table 8", Capacity = 6, Status = TableStatus.Occupied }
            };
        }

        private void LoadMockItems()
        {
            Items = new List<MenuItemDto>
            {
                new() { Id = 1, BusinessId = BusinessId, Name = "Beef burger", Description = "House patty, cheddar, pickles", Price = 8.89m, Tags = new() { "Mains" } },
                new() { Id = 2, BusinessId = BusinessId, Name = "Pork burger", Description = "Brioche, slaw, smoky sauce", Price = 9.59m, Tags = new() { "Mains" } },
                new() { Id = 3, BusinessId = BusinessId, Name = "Chicken burger", Description = "Crispy chicken, aioli", Price = 9.19m, Tags = new() { "Mains" } },
                new() { Id = 4, BusinessId = BusinessId, Name = "Fish burger", Description = "Fried cod, tartar", Price = 9.89m, Tags = new() { "Mains", "Fish" } },
                new() { Id = 5, BusinessId = BusinessId, Name = "Cheese pizza", Description = "Four cheese blend", Price = 10.49m, Tags = new() { "Mains" } },
                new() { Id = 6, BusinessId = BusinessId, Name = "Pepperoni pizza", Description = "Spicy pepperoni, basil", Price = 10.59m, Tags = new() { "Mains" } },
                new() { Id = 7, BusinessId = BusinessId, Name = "Vegetarian pizza", Description = "Seasonal vegetables", Price = 9.99m, Tags = new() { "Mains" } },
                new() { Id = 8, BusinessId = BusinessId, Name = "Appetizer platter", Description = "Wings, fries, dips", Price = 12.49m, Tags = new() { "Appetizers" } },
                new() { Id = 9, BusinessId = BusinessId, Name = "Caesar salad", Description = "Romaine, croutons", Price = 7.59m, Tags = new() { "Salads" } },
                new() { Id = 10, BusinessId = BusinessId, Name = "Tomato soup", Description = "Creamy basil", Price = 6.49m, Tags = new() { "Soups" } },
                new() { Id = 11, BusinessId = BusinessId, Name = "Still water", Description = "0.5L bottle", Price = 3.2m, Tags = new() { "Drinks" } },
                new() { Id = 12, BusinessId = BusinessId, Name = "House red wine", Description = "Glass", Price = 6.9m, Tags = new() { "Alcohol" } }
            };
        }

        private void LoadMockOrders()
        {
            Orders = new List<OrderDto>
            {
                new()
                {
                    Id = 101,
                    BusinessId = BusinessId,
                    TableId = 8,
                    Status = OrderStatus.Open,
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-12),
                    Items = new List<OrderItemDto>
                    {
                        new() { Id = 1, ItemId = 1, Quantity = 1 },
                        new() { Id = 2, ItemId = 6, Quantity = 1 },
                        new() { Id = 3, ItemId = 11, Quantity = 2 }
                    }
                },
                new()
                {
                    Id = 102,
                    BusinessId = BusinessId,
                    TableId = 4,
                    Status = OrderStatus.Open,
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-40),
                    Items = new List<OrderItemDto>
                    {
                        new() { Id = 4, ItemId = 4, Quantity = 2 },
                        new() { Id = 5, ItemId = 9, Quantity = 1 }
                    }
                },
                new()
                {
                    Id = 103,
                    BusinessId = BusinessId,
                    TableId = 3,
                    Status = OrderStatus.Closed,
                    UpdatedAt = DateTime.UtcNow.AddHours(-1),
                    Items = new List<OrderItemDto>
                    {
                        new() { Id = 6, ItemId = 5, Quantity = 1 },
                        new() { Id = 7, ItemId = 12, Quantity = 1 }
                    },
                    Tip = 2,
                    Payments = new List<PaymentDto>
                    {
                        new() { Id = 9001, Method = PaymentMethod.Card, Amount = 19.39m, Status = PaymentStatus.Completed, PaidAt = DateTime.UtcNow.AddMinutes(-10) }
                    }
                }
            };

            ActiveOrder = Orders.FirstOrDefault();
            SelectedTableId = ActiveOrder?.TableId;
        }

        private void SyncTableStatuses()
        {
            foreach (var table in Tables)
            {
                var hasOpen = Orders.Any(o => o.TableId == table.Id && IsOpenStatus(o.Status));
                if (hasOpen)
                {
                    table.Status = TableStatus.Occupied;
                }
                else if (table.Status != TableStatus.Reserved)
                {
                    table.Status = TableStatus.Free;
                }
            }
        }
    }
}
