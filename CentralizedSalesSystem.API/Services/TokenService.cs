using CentralizedSalesSystem.API.Authorization;
using CentralizedSalesSystem.API.Models;
using CentralizedSalesSystem.API.Models.Auth.enums;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CentralizedSalesSystem.API.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
    }

    public sealed class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JWT:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryMinutes = int.TryParse(_config["JWT:ExpiryMinutes"], out var minutes)
                ? minutes
                : 60;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new("businessId", user.BusinessId.ToString())
            };

            var activePermissionCodes = user.UserRoles
                .Select(ur => ur.Role)
                .Where(r => r.Status == Status.Active)
                .SelectMany(r => r.RolePermissions)
                .Where(rp => rp.Permission.Status == Status.Active)
                .Select(rp => rp.Permission.Code.ToUpperInvariant())
                .Distinct()
                .ToList();

            foreach (var userRole in user.UserRoles.Where(ur => ur.Role.Status == Status.Active))
            {
                claims.Add(new(ClaimTypes.Role, userRole.Role.Title));
            }

            foreach (var code in activePermissionCodes)
            {
                claims.Add(new(PermissionAuthorizationHandler.PermissionClaimType, code));
                // Legacy name kept for backward compatibility with any existing consumers
                claims.Add(new(PermissionAuthorizationHandler.LegacyPermissionClaimType, code));
            }

            var token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
