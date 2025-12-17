using Microsoft.AspNetCore.Authorization;

namespace CentralizedSalesSystem.API.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class AuthorizePermissionAttribute : AuthorizeAttribute
    {
        public AuthorizePermissionAttribute(string code)
        {
            Policy = $"{PermissionPolicyProvider.PolicyPrefix}{code}";
        }
    }
}
