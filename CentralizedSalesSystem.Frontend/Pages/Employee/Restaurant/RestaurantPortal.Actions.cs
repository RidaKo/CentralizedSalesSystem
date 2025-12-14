using System;
using System.Linq;
using System.Threading.Tasks;
using CentralizedSalesSystem.Frontend.Models;
using MudBlazor;

namespace CentralizedSalesSystem.Frontend.Pages.Employee.Restaurant
{
    public partial class RestaurantPortal
    {
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

        private Task OnSearchChanged(string? text)
        {
            SearchText = text;
            return Task.CompletedTask;
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

        private void OnOrderSelected(OrderDto order)
        {
            ActiveOrder = order;
            SelectedTableId = order.TableId;
            SelectedTableForNewOrder = order.TableId;
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
    }
}
