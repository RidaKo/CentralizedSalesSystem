using CentralizedSalesSystem.API.Models.Auth.enums;
using Microsoft.AspNetCore.Authorization;

namespace CentralizedSalesSystem.API.Authorization
{
    public sealed class PermissionAuthorizationHandler
        : AuthorizationHandler<PermissionRequirement>
    {
        public const string PermissionClaimType = "perm";
        public const string LegacyPermissionClaimType = "permission";
        private const string SuperPermission = nameof(PermissionCode.MANAGE_ALL);

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var hasSuper = context.User.HasClaim(
                c => (c.Type == PermissionClaimType || c.Type == LegacyPermissionClaimType)
                     && string.Equals(c.Value, SuperPermission, StringComparison.OrdinalIgnoreCase));

            if (hasSuper ||
                context.User.HasClaim(
                    c => (c.Type == PermissionClaimType || c.Type == LegacyPermissionClaimType)
                         && string.Equals(c.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
