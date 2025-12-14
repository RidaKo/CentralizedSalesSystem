using CentralizedSalesSystem.API.Models;
using CentralizedSalesSystem.API.Models.Auth;
using CentralizedSalesSystem.API.Models.Auth.enums;
using CentralizedSalesSystem.API.Models.Business;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CentralizedSalesSystem.API.Data
{
    public class DbSeeder
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DbSeeder(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var scopedProvider = scope.ServiceProvider;
            var context = scopedProvider.GetRequiredService<CentralizedSalesDbContext>();
            var passwordHasher = scopedProvider.GetRequiredService<IPasswordHasher<User>>();
            var logger = scopedProvider.GetRequiredService<ILogger<CentralizedSalesDbContext>>();
    
            await MigrateWithRecoveryAsync(context, logger, cancellationToken);

            // Seed only when empty to avoid overriding user data.
            if (await context.Users.AnyAsync(cancellationToken))
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;

            await using var tx = await context.Database.BeginTransactionAsync(cancellationToken);

            var manageAllPermission = new Permission
            {
                Title = "Manage All",
                Description = "Seeded permission for broad access",
                Code = "manage_all",
                Resource = PermissionResource.Business,
                CreatedAt = now,
                UpdatedAt = now,
                Status = Status.Active
            };

            // Create a business and user together to satisfy FK constraints
            var business = new Business
            {
                Name = "Seed Business",
                Phone = "+10000000000",
                Address = "123 Seed St",
                Email = "owner@ex.com",
                Country = "N/A",
                Currency = Currency.EUR,
                SubscriptionPlan = SubscriptionPlan.Catering
            };

            var adminUser = new User
            {
                Name = "Admin",
                Email = "admin@ex.com",
                Phone = "+10000000000",
                Status = Status.Active,
                Business = business,
                UserRoles = new List<UserRole>()
            };

            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "pass");

            var adminRole = new Role
            {
                Title = "Admin",
                Description = "Seeded administrator role",
                BussinessId = 0, // set after business is persisted
                CreatedAt = now,
                UpdatedAt = now,
                Status = Status.Active,
                RolePermissions = new List<RolePermission>()
            };

            var rolePermission = new RolePermission
            {
                Role = adminRole,
                Permission = manageAllPermission,
                CreatedAt = now,
                UpdatedAt = now
            };

            adminRole.RolePermissions.Add(rolePermission);

            var userRole = new UserRole
            {
                User = adminUser,
                Role = adminRole,
                AssignedAt = now
            };

            adminUser.UserRoles.Add(userRole);

            await context.AddRangeAsync(
                new object[] { manageAllPermission, business, adminRole, adminUser, rolePermission, userRole },
                cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            // Update foreign keys that depend on generated IDs
            adminRole.BussinessId = business.Id;
            business.OwnerId = adminUser.Id;
            context.Roles.Update(adminRole);
            context.Businesses.Update(business);

            await context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }

        private async Task MigrateWithRecoveryAsync(CentralizedSalesDbContext context, ILogger logger, CancellationToken cancellationToken)
        {
            try
            {
                await context.Database.MigrateAsync(cancellationToken);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                logger.LogWarning(ex, "Migration failed due to FK/data issues. Dropping and recreating database for a clean dev seed.");
                await context.Database.EnsureDeletedAsync(cancellationToken);
                await context.Database.MigrateAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Migration failed and recovery was not attempted.");
                throw;
            }
        }
    }
}
