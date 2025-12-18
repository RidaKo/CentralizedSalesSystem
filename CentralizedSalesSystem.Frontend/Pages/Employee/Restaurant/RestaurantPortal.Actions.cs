using System;
using System.Linq;
using System.Threading.Tasks;
using CentralizedSalesSystem.Frontend.Json;
using CentralizedSalesSystem.Frontend.Models;
using MudBlazor;

namespace CentralizedSalesSystem.Frontend.Pages.Employee.Restaurant
{
    public partial class RestaurantPortal
    {
        private async Task CreateOrderFromSelectionAsync()
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

            try
            {
                var createDto = new
                {
                    BusinessId = BusinessId,
                    UserId = CurrentUserId,
                    TableId = tableId,
                    Status = "Open"
                };

                var response = await Http.PostAsJsonAsync("orders", createDto);
                if (response.IsSuccessStatusCode)
                {
                    var createdOrder = await response.Content.ReadFromJsonAsync<OrderDto>();
                    if (createdOrder != null)
                    {
                        Orders.Insert(0, createdOrder);
                        ActiveOrder = createdOrder;
                        SelectedTableId = tableId;
                        SelectedOrderDiscountId = createdOrder.DiscountId;
                        UpdateTableStatus(tableId, TableStatus.Occupied);
                        ActiveView = PortalView.CurrentOrder;
                        IsEditMode = true;
                        
                        var table = Tables.FirstOrDefault(t => t.Id == tableId);
                        Snackbar.Add($"Created new order for {table?.Name ?? $"Table {tableId}"}", Severity.Success);
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Snackbar.Add($"Failed to create order: {errorContent}", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error creating order: {ex.Message}", Severity.Error);
            }
        }

        private Task CreateOrderFromSelection() => CreateOrderFromSelectionAsync();

        private async Task AddItemToOrderAsync(MenuItemDto menuItem)
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
                // Create new order item
                try
                {
                    var createDto = new
                    {
                        OrderId = ActiveOrder.Id,
                        ItemId = menuItem.Id,
                        Quantity = 1,
                        Notes = (string?)null,
                        DiscountId = (long?)null,
                        TaxId = (long?)null,
                        ServiceChargeId = (long?)null
                    };

                    var response = await Http.PostAsJsonAsync("orderItems", createDto);
                    if (response.IsSuccessStatusCode)
                    {
                        var createdItem = await response.Content.ReadFromJsonAsync<OrderItemDto>();
                        if (createdItem != null)
                        {
                            ActiveOrder.Items.Add(createdItem);
                            ActiveOrder.UpdatedAt = DateTimeOffset.UtcNow;
                        }
                    }
                    else
                    {
                        Snackbar.Add("Failed to add item to order", Severity.Error);
                    }
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"Error adding item: {ex.Message}", Severity.Error);
                }
            }
            else
            {
                if (existingLine.Quantity >= 2 && !IsEditMode)
                {
                    Snackbar.Add("Reached quick-add limit (2). Use Edit to change quantity.", Severity.Info);
                    return;
                }
                
                // Update existing order item
                await UpdateOrderItemQuantityAsync(existingLine, existingLine.Quantity + 1);
            }
        }

        private Task AddItemToOrder(MenuItemDto menuItem) => AddItemToOrderAsync(menuItem);

        private async Task UpdateOrderItemQuantityAsync(OrderItemDto line, int newQuantity)
        {
            try
            {
                var updateDto = new
                {
                    Quantity = Math.Clamp(newQuantity, 1, 10)
                };

                var response = await Http.PatchAsJsonAsync($"orderItems/{line.Id}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    line.Quantity = updateDto.Quantity;
                    if (ActiveOrder != null)
                    {
                        ActiveOrder.UpdatedAt = DateTimeOffset.UtcNow;
                    }
                }
                else
                {
                    Snackbar.Add("Failed to update quantity", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error updating quantity: {ex.Message}", Severity.Error);
            }
        }

        private Task ChangeQuantity(OrderItemDto line, int delta)
        {
            if (!CanModifyActiveOrder || ActiveOrder is null) return Task.CompletedTask;
            var newQuantity = Math.Clamp(line.Quantity + delta, 1, 10);
            return UpdateOrderItemQuantityAsync(line, newQuantity);
        }

        private async Task RemoveItemAsync(OrderItemDto line)
        {
            if (!CanModifyActiveOrder || ActiveOrder is null) return;
            
            try
            {
                var response = await Http.DeleteAsync($"orderItems/{line.Id}");
                if (response.IsSuccessStatusCode)
                {
                    ActiveOrder.Items.Remove(line);
                    ActiveOrder.UpdatedAt = DateTimeOffset.UtcNow;
                }
                else
                {
                    Snackbar.Add("Failed to remove item", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error removing item: {ex.Message}", Severity.Error);
            }
        }

        private Task RemoveItem(OrderItemDto line) => RemoveItemAsync(line);

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
            SelectedOrderDiscountId = order.DiscountId;
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

        private async Task SendOrderAsync()
        {
            if (ActiveOrder is null) return;
            
            try
            {
                var updateDto = new
                {
                    Status = "Pending"
                };

                var response = await Http.PatchAsJsonAsync($"orders/{ActiveOrder.Id}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    ActiveOrder.Status = OrderStatus.Pending;
                    ActiveOrder.UpdatedAt = DateTimeOffset.UtcNow;
                    Snackbar.Add("Order sent to the kitchen.", Severity.Success);
                }
                else
                {
                    Snackbar.Add("Failed to send order", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error sending order: {ex.Message}", Severity.Error);
            }
        }

        private Task SendOrder() => SendOrderAsync();

        private async Task CancelOrderAsync()
        {
            if (ActiveOrder is null || !CanModifyActiveOrder) return;
            
            try
            {
                var updateDto = new
                {
                    Status = "Closed"
                };

                var response = await Http.PatchAsJsonAsync($"orders/{ActiveOrder.Id}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    ActiveOrder.Status = OrderStatus.Closed;
                    ActiveOrder.UpdatedAt = DateTimeOffset.UtcNow;
                    UpdateTableStatus(ActiveOrder.TableId, TableStatus.Free);
                    SyncTableStatuses();
                    IsEditMode = false;
                    Snackbar.Add("Order cancelled.", Severity.Warning);
                }
                else
                {
                    Snackbar.Add("Failed to cancel order", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error cancelling order: {ex.Message}", Severity.Error);
            }
        }

        private Task CancelOrder() => CancelOrderAsync();

        private async Task ApplyOrderDiscountAsync()
        {
            if (ActiveOrder is null || !CanModifyActiveOrder) return;

            var previousDiscountId = ActiveOrder.DiscountId;
            var discountId = SelectedOrderDiscountId ?? 0;

            try
            {
                var updateDto = new
                {
                    DiscountId = discountId
                };

                var response = await Http.PatchAsJsonAsync($"orders/{ActiveOrder.Id}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    ActiveOrder.DiscountId = discountId == 0 ? null : discountId;
                    ActiveOrder.Discount = discountId == 0
                        ? null
                        : Discounts.FirstOrDefault(d => d.Id == discountId);
                    ActiveOrder.UpdatedAt = DateTimeOffset.UtcNow;
                    Snackbar.Add(discountId == 0 ? "Discount cleared." : "Discount applied.", Severity.Success);
                }
                else
                {
                    SelectedOrderDiscountId = previousDiscountId;
                    Snackbar.Add("Failed to apply discount", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                SelectedOrderDiscountId = previousDiscountId;
                Snackbar.Add($"Error applying discount: {ex.Message}", Severity.Error);
            }
        }

        private Task ApplyOrderDiscount() => ApplyOrderDiscountAsync();

        private async Task ApplyItemDiscountAsync(OrderItemDto line, long? discountId)
        {
            if (!CanModifyActiveOrder) return;

            var previousDiscountId = line.DiscountId;
            var discountValue = discountId ?? 0;

            try
            {
                var updateDto = new
                {
                    DiscountId = discountValue
                };

                var response = await Http.PatchAsJsonAsync($"orderItems/{line.Id}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    line.DiscountId = discountValue == 0 ? null : discountValue;
                    line.Discount = discountValue == 0
                        ? null
                        : Discounts.FirstOrDefault(d => d.Id == discountValue);
                    if (ActiveOrder != null)
                    {
                        ActiveOrder.UpdatedAt = DateTimeOffset.UtcNow;
                    }
                    Snackbar.Add(discountValue == 0 ? "Item discount cleared." : "Item discount applied.", Severity.Success);
                }
                else
                {
                    line.DiscountId = previousDiscountId;
                    Snackbar.Add("Failed to apply item discount", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                line.DiscountId = previousDiscountId;
                Snackbar.Add($"Error applying item discount: {ex.Message}", Severity.Error);
            }
        }

        private Task ApplyItemDiscount(OrderItemDto line, long? discountId) =>
            ApplyItemDiscountAsync(line, discountId);

        private async Task ApplyTipAsync(decimal? tipAmount)
        {
            if (ActiveOrder is null || !CanModifyActiveOrder) return;
            
            try
            {
                var updateDto = new
                {
                    Tip = tipAmount
                };

                var response = await Http.PatchAsJsonAsync($"orders/{ActiveOrder.Id}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    ActiveOrder.Tip = tipAmount ?? 0;
                    ActiveOrder.UpdatedAt = DateTimeOffset.UtcNow;
                }
                else
                {
                    Snackbar.Add("Failed to apply tip", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error applying tip: {ex.Message}", Severity.Error);
            }
        }

        private Task ApplyTipPercent(decimal percent)
        {
            if (ActiveOrder is null || !CanModifyActiveOrder) return Task.CompletedTask;
            var tipAmount = Math.Round(Subtotal * percent, 2);
            return ApplyTipAsync(tipAmount);
        }

        private Task ApplyCustomTip()
        {
            if (ActiveOrder is null || !CanModifyActiveOrder) return Task.CompletedTask;
            var tipAmount = Math.Max(0, TipInput);
            return ApplyTipAsync(tipAmount);
        }

        private async Task AddPaymentAsync()
        {
            if (ActiveOrder is null || !CanModifyActiveOrder) return;

            var amount = NewPaymentAmount > 0 ? NewPaymentAmount : RemainingToPay;
            if (amount <= 0) return;

            try
            {
                var createDto = new
                {
                    OrderId = ActiveOrder.Id,
                    Amount = amount,
                    Method = NewPaymentMethod,
                    Provider = 0, // Default provider
                    Currency = PaymentCurrency.EUR,
                    Status = PaymentStatus.Completed,
                    PaidAt = DateTimeOffset.UtcNow,
                    BussinesId = BusinessId,
                    GiftCardId = (long?)null
                };

                var response = await Http.PostAsJsonAsync("payments", createDto);
                if (response.IsSuccessStatusCode)
                {
                    var createdPayment = await response.Content.ReadFromJsonAsync<PaymentDto>();
                    if (createdPayment != null)
                    {
                        ActiveOrder.Payments.Add(createdPayment);
                        ActiveOrder.UpdatedAt = DateTimeOffset.UtcNow;
                        NewPaymentAmount = 0;

                        if (TotalPaid >= CurrentOrderTotal)
                        {
                            await CloseOrderAsync();
                        }
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Snackbar.Add($"Failed to add payment: {errorContent}", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error adding payment: {ex.Message}", Severity.Error);
            }
        }

        private Task AddPayment() => AddPaymentAsync();

        private async Task CloseOrderAsync()
        {
            if (ActiveOrder is null) return;
            
            try
            {
                var updateDto = new
                {
                    Status = "Closed"
                };

                var response = await Http.PatchAsJsonAsync($"orders/{ActiveOrder.Id}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    ActiveOrder.Status = OrderStatus.Closed;
                    UpdateTableStatus(ActiveOrder.TableId, TableStatus.Free);
                    SyncTableStatuses();
                    IsEditMode = false;
                }
                else
                {
                    Snackbar.Add("Failed to close order", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error closing order: {ex.Message}", Severity.Error);
            }
        }

        private async Task PayOrder()
        {
            if (ActiveOrder is null || RemainingToPay <= 0 || !CanModifyActiveOrder) return;
            NewPaymentAmount = RemainingToPay;
            await AddPaymentAsync();
            Snackbar.Add("Order marked as paid.", Severity.Success);
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
            var tax = GetTaxAmount(target);
            var total = CalculateTotal(target);

            var text = string.Join("\n", lines)
                       + $"\nSubtotal: {subtotal:C}"
                       + $"\nDiscount: {discount:C}"
                       + $"\nTax: {tax:C}"
                       + $"\nTip: {target.Tip:C}"
                       + $"\nTotal: {total:C}"
                       + $"\n\nPayments:\n{payments}";

            await DialogService.ShowMessageBox("Receipt", text, yesText: "Close");
        }

        private async Task RefundOrderAsync(OrderDto? order = null)
        {
            var target = order ?? ActiveOrder;
            if (target is null || !CanRefund(target)) return;
            
            try
            {
                var updateDto = new
                {
                    Status = "Refunded"
                };

                var response = await Http.PatchAsJsonAsync($"orders/{target.Id}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    target.Status = OrderStatus.Refunded;
                    target.UpdatedAt = DateTimeOffset.UtcNow;
                    UpdateTableStatus(target.TableId, TableStatus.Free);
                    SyncTableStatuses();
                    Snackbar.Add("Order marked as refunded.", Severity.Warning);
                }
                else
                {
                    Snackbar.Add("Failed to refund order", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error refunding order: {ex.Message}", Severity.Error);
            }
        }

        private Task RefundOrder(OrderDto? order = null) => RefundOrderAsync(order);

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
