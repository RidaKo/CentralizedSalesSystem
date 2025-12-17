using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CentralizedSalesSystem.Frontend.Services;

public sealed class ApiAuthStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly ITokenStore _tokenStore;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public ApiAuthStateProvider(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenStore.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return Anonymous;
        }

        var state = BuildAuthenticationState(token);
        return state;
    }

    public void NotifyUserLoggedOut()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    public void NotifyUserAuthenticated(string token)
    {
        var authState = BuildAuthenticationState(token);
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    private AuthenticationState BuildAuthenticationState(string token)
    {
        try
        {
            var jwt = _tokenHandler.ReadJwtToken(token);

            if (jwt.ValidTo < DateTime.UtcNow)
            {
                return Anonymous;
            }

            var identity = new ClaimsIdentity(jwt.Claims, authenticationType: "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return Anonymous;
        }
    }
}
