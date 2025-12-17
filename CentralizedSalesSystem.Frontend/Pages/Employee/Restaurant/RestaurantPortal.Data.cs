using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CentralizedSalesSystem.Frontend.Json;
using CentralizedSalesSystem.Frontend.Models;
using MudBlazor;

namespace CentralizedSalesSystem.Frontend.Pages.Employee.Restaurant
{
    public partial class RestaurantPortal
    {
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
                if (Tables.Count == 0)
                {
                    LoadMockTables();
                }
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
