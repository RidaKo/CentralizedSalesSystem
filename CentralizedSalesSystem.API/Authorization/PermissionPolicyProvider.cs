using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CentralizedSalesSystem.API.Authorization
{
    public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        public const string PolicyPrefix = "Permission:";

        private readonly DefaultAuthorizationPolicyProvider _fallback;

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallback = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var code = policyName.Substring(PolicyPrefix.Length);
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PermissionRequirement(code))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return _fallback.GetPolicyAsync(policyName);
        }
    }
}
