using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using MudBlazor;
using Microsoft.AspNetCore.Components.Authorization;
using CentralizedSalesSystem.Frontend.Json;
using CentralizedSalesSystem.Frontend.Models;

namespace CentralizedSalesSystem.Frontend.Services;

public sealed class BusinessContext
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly HttpClient _httpClient;
    private readonly ISnackbar _snackbar;

    private bool _loaded;
    private long? _businessId;
    private long? _ownerId;
    private long? _currentUserId;

    public SubscriptionPlan? SubscriptionPlan { get; private set; }
    public bool IsRestaurantEnabled { get; private set; }
    public bool IsBeautyEnabled { get; private set; }
    public bool IsOwner { get; private set; }
    public bool IsSuper { get; private set; }
    public long? BusinessId => _businessId;
    public long? OwnerId => _ownerId;
    public event Action? Changed;

    public BusinessContext(
        AuthenticationStateProvider authStateProvider,
        HttpClient httpClient,
        ISnackbar snackbar)
    {
        _authStateProvider = authStateProvider;
        _httpClient = httpClient;
        _snackbar = snackbar;
    }

    public async Task EnsureLoadedAsync()
    {
        if (_loaded) return;

        _loaded = true;
        ClearFlags();

        var state = await _authStateProvider.GetAuthenticationStateAsync();
        _currentUserId = GetUserId(state.User);
        IsSuper = HasSuperPermission(state.User);

        var businessIdValue = state.User.FindFirst("businessId")?.Value;

        if (string.IsNullOrWhiteSpace(businessIdValue) || !long.TryParse(businessIdValue, out var businessId))
        {
            NotifyChanged();
            return;
        }

        _businessId = businessId;

        try
        {
            var response = await _httpClient.GetAsync($"businesses/{businessId}");
            if (!response.IsSuccessStatusCode)
            {
                // If the user cannot fetch business details (likely lacks BUSINESS_VIEW), still enable employee portals.
                if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    IsRestaurantEnabled = true;
                    IsBeautyEnabled = true;
                    NotifyChanged();
                }
                return;
            }

            var business = await HttpJsonDefaultsExtensions.ReadFromJsonAsync<BusinessDto>(response.Content);
            if (business is null)
            {
                IsRestaurantEnabled = true;
                IsBeautyEnabled = true;
                NotifyChanged();
                return;
            }

            _ownerId = business.Owner;
            SetFlags(business.SubscriptionPlan);
            SetOwnership();
            NotifyChanged();
        }
        catch (Exception ex)
        {
            _snackbar.Add($"Unable to load subscription info: {ex.Message}", Severity.Warning);
            // Fallback to show portals if we cannot load business info.
            IsRestaurantEnabled = true;
            IsBeautyEnabled = true;
            NotifyChanged();
        }
    }

    public async Task RefreshAsync()
    {
        _loaded = false;
        ClearFlags();
        await EnsureLoadedAsync();
    }

    public void Reset()
    {
        _loaded = false;
        ClearFlags();
    }

    private void SetFlags(SubscriptionPlan? subscriptionPlan)
    {
        if (!subscriptionPlan.HasValue)
        {
            return;
        }

        SubscriptionPlan = subscriptionPlan;

        IsRestaurantEnabled = subscriptionPlan == Models.SubscriptionPlan.Catering;
        IsBeautyEnabled = subscriptionPlan == Models.SubscriptionPlan.Beauty;
    }

    private void ClearFlags()
    {
        SubscriptionPlan = null;
        IsRestaurantEnabled = false;
        IsBeautyEnabled = false;
        IsOwner = false;
        _businessId = null;
        _ownerId = null;
        _currentUserId = null;
        IsSuper = false;
        NotifyChanged();
    }

    private void SetOwnership()
    {
        IsOwner = _ownerId.HasValue && _currentUserId.HasValue && _ownerId.Value == _currentUserId.Value;
    }

    private static long? GetUserId(ClaimsPrincipal user)
    {
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? user.FindFirst("sub")?.Value;

        return long.TryParse(id, out var parsed) ? parsed : null;
    }

    private static bool HasSuperPermission(ClaimsPrincipal user) =>
        user.Claims.Any(c =>
            (c.Type == "perm" || c.Type == "permission") &&
            string.Equals(c.Value, "MANAGE_ALL", StringComparison.OrdinalIgnoreCase));

    public void AssumeBusiness(long businessId, SubscriptionPlan? subscriptionPlan)
    {
        if (!IsSuper) return;

        _businessId = businessId;
        _ownerId = null;
        _currentUserId = null;

        IsOwner = true;
        SubscriptionPlan = null;
        SetFlags(subscriptionPlan);
        NotifyChanged();
    }

    private sealed class BusinessDto
    {
        public long Id { get; set; }
        public SubscriptionPlan? SubscriptionPlan { get; set; }
        public long? Owner { get; set; }
    }

    private void NotifyChanged() => Changed?.Invoke();
}
