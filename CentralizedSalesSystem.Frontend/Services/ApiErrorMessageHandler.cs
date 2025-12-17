using System.Net;
using MudBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CentralizedSalesSystem.Frontend.Services;

public sealed class ApiErrorMessageHandler : DelegatingHandler
{
    private readonly ISnackbar _snackbar;
    private readonly NavigationManager _navigation;
    private readonly ITokenStore _tokenStore;
    private readonly AuthenticationStateProvider _authStateProvider;

    public ApiErrorMessageHandler(
        ISnackbar snackbar,
        NavigationManager navigation,
        ITokenStore tokenStore,
        AuthenticationStateProvider authStateProvider)
    {
        _snackbar = snackbar;
        _navigation = navigation;
        _tokenStore = tokenStore;
        _authStateProvider = authStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        var path = request.RequestUri?.AbsolutePath?.ToLowerInvariant() ?? string.Empty;
        var isAuthEndpoint = path.StartsWith("/auth");

        // Let auth endpoints surface their own errors to the caller so login/register can show messages.
        if (isAuthEndpoint)
        {
            return response;
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _snackbar.Add("Session expired. Please sign in again.", Severity.Error);
            await _tokenStore.ClearTokenAsync();

            if (_authStateProvider is ApiAuthStateProvider apiAuthStateProvider)
            {
                apiAuthStateProvider.NotifyUserLoggedOut();
            }

            _navigation.NavigateTo("/login", forceLoad: true);
        }
        else if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            _snackbar.Add("You don't have permission to perform this action.", Severity.Error);
        }

        return response;
    }
}
