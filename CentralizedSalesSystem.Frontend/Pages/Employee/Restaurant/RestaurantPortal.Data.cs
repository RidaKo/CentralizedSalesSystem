using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
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
            await InitializeContextAsync();
            await LoadDataAsync();
        }

        private async Task InitializeContextAsync()
        {
            await BusinessContext.EnsureLoadedAsync();
            BusinessId = BusinessContext.BusinessId ?? 1;

            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? authState.User.FindFirst("sub")?.Value;
            
            if (long.TryParse(userIdClaim, out var userId))
            {
                CurrentUserId = userId;
            }
            else
            {
                Snackbar.Add("Unable to determine current user.", Severity.Error);
                CurrentUserId = 1; // Fallback
            }
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                await Task.WhenAll(LoadTablesAsync(), LoadItemsAsync(), LoadTaxesAsync(), LoadDiscountsAsync(), LoadOrdersAsync());
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
                var response = await HttpJsonDefaultsExtensions.GetFromJsonAsync<PaginatedResponse<TableDto>>(
                    Http,
                    $"tables?limit=200&filterByBusinessId={BusinessId}");
                Tables = response?.Data ?? new List<TableDto>();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to load tables: {ex.Message}", Severity.Error);
                Tables = new List<TableDto>();
            }
        }

        private async Task LoadItemsAsync()
        {
            try
            {
                var response = await HttpJsonDefaultsExtensions.GetFromJsonAsync<PaginatedResponse<MenuItemDto>>(
                    Http,
                    $"items?limit=200&filterByBusinessId={BusinessId}");
                Items = response?.Data ?? new List<MenuItemDto>();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to load items: {ex.Message}", Severity.Error);
                Items = new List<MenuItemDto>();
            }
        }

        private async Task LoadTaxesAsync()
        {
            try
            {
                var response = await HttpJsonDefaultsExtensions.GetFromJsonAsync<PaginatedResponse<Tax>>(
                    Http,
                    $"taxes?limit=200&filterByBusinessId={BusinessId}");
                Taxes = response?.Data ?? new List<Tax>();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to load taxes: {ex.Message}", Severity.Error);
                Taxes = new List<Tax>();
            }
        }

        private async Task LoadDiscountsAsync()
        {
            try
            {
                var response = await HttpJsonDefaultsExtensions.GetFromJsonAsync<PaginatedResponse<Discount>>(
                    Http,
                    $"discounts?limit=200&filterByBusinessId={BusinessId}");
                Discounts = response?.Data ?? new List<Discount>();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to load discounts: {ex.Message}", Severity.Error);
                Discounts = new List<Discount>();
            }
        }

        private async Task LoadOrdersAsync()
        {
            try
            {

                var response = await HttpJsonDefaultsExtensions.GetFromJsonAsync<PaginatedResponse<OrderDto>>(
                    Http, $"orders?limit=100&filterByBusinessId={BusinessId}&sortBy=UpdatedAt&sortDirection=desc&includeItems=true");
                Orders = response?.Data ?? new List<OrderDto>();
                
                // Ensure each order has its items populated
                foreach (var order in Orders)
                {
                    order.Items ??= new List<OrderItemDto>();
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to load orders: {ex.Message}", Severity.Error);
                Orders = new List<OrderDto>();
            }
        }

        private void EnsureSelections()
        {
            var initialTable = ActiveOrder?.TableId ?? Tables.FirstOrDefault()?.Id;
            SetActiveOrderForTable(initialTable);
            ActiveOrder ??= Orders.FirstOrDefault(o => IsOpenStatus(o.Status)) ?? Orders.FirstOrDefault();
            ActiveView = PortalView.CurrentOrder;
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
