using CentralizedSalesSystem.API.Models;
using CentralizedSalesSystem.API.Models.Auth;
using CentralizedSalesSystem.API.Models.Auth.enums;
using CentralizedSalesSystem.API.Models.Business;
using CentralizedSalesSystem.API.Models.Orders;
using CentralizedSalesSystem.API.Models.Orders.enums;
using CentralizedSalesSystem.API.Models.Reservations;
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
                return;

            var now = DateTimeOffset.UtcNow;

            await using var tx = await context.Database.BeginTransactionAsync(cancellationToken);

            // -------------------------
            // 0) CREATE IN-MEMORY OBJECTS (same as your code)
            // -------------------------

            // Shared permissions
            var permissions = new List<Permission>
    {
        new Permission
        {
            Title = "Manage All",
            Description = "Full access to business resources",
            Code = "manage_all",
            Resource = PermissionResource.Business,
            CreatedAt = now,
            UpdatedAt = now,
            Status = Status.Active
        },
        new Permission
        {
            Title = "Manage Orders",
            Description = "Can create and update orders",
            Code = "orders_manage",
            Resource = PermissionResource.Order,
            CreatedAt = now,
            UpdatedAt = now,
            Status = Status.Active
        },
        new Permission
        {
            Title = "Manage Products",
            Description = "Can manage items and variations",
            Code = "products_manage",
            Resource = PermissionResource.Product,
            CreatedAt = now,
            UpdatedAt = now,
            Status = Status.Active
        },
        new Permission
        {
            Title = "Manage Reservations",
            Description = "Can manage reservations",
            Code = "reservations_manage",
            Resource = PermissionResource.Reservation,
            CreatedAt = now,
            UpdatedAt = now,
            Status = Status.Active
        }
    };

            // Business 1: Sunrise Cafe (Catering)
            var sunrise = new Business
            {
                Name = "Sunrise Cafe",
                Phone = "+15550001111",
                Address = "123 Market St, Springfield",
                Email = "owner@sunrisecafe.com",
                Country = "USA",
                Currency = Currency.USD,
                SubscriptionPlan = SubscriptionPlan.Catering,
                WorkingHours = "08:00-20:00",
                NextPaymentDueDate = now.AddDays(30)
            };

            var sunriseOwner = new User
            {
                Name = "Sarah Sunrise",
                Email = "sarah@sunrisecafe.com",
                Phone = "+15550002222",
                Status = Status.Active
            };
            sunriseOwner.PasswordHash = passwordHasher.HashPassword(sunriseOwner, "owner123");

            var sunriseStaff = new User
            {
                Name = "Omar Waits",
                Email = "omar@sunrisecafe.com",
                Phone = "+15550003333",
                Status = Status.Active
            };
            sunriseStaff.PasswordHash = passwordHasher.HashPassword(sunriseStaff, "staff123");

            var sunriseOwnerRole = new Role
            {
                Title = "Cafe Owner",
                Description = "Owner with full permissions",
                CreatedAt = now,
                UpdatedAt = now,
                Status = Status.Active
            };

            var sunriseStaffRole = new Role
            {
                Title = "Cafe Staff",
                Description = "Staff who manage orders and reservations",
                CreatedAt = now,
                UpdatedAt = now,
                Status = Status.Active
            };

            var sunriseRolePermissions = new List<RolePermission>
    {
        new RolePermission { Role = sunriseOwnerRole, Permission = permissions[0], CreatedAt = now, UpdatedAt = now },
        new RolePermission { Role = sunriseOwnerRole, Permission = permissions[1], CreatedAt = now, UpdatedAt = now },
        new RolePermission { Role = sunriseOwnerRole, Permission = permissions[2], CreatedAt = now, UpdatedAt = now },
        new RolePermission { Role = sunriseOwnerRole, Permission = permissions[3], CreatedAt = now, UpdatedAt = now },
        new RolePermission { Role = sunriseStaffRole, Permission = permissions[1], CreatedAt = now, UpdatedAt = now },
        new RolePermission { Role = sunriseStaffRole, Permission = permissions[3], CreatedAt = now, UpdatedAt = now }
    };

            var sunriseUserRoles = new List<UserRole>
    {
        new UserRole { User = sunriseOwner, Role = sunriseOwnerRole, AssignedAt = now },
        new UserRole { User = sunriseStaff, Role = sunriseStaffRole, AssignedAt = now }
    };

            var sunriseItems = new List<Item>
    {
        new Item
        {
            Name = "House Coffee",
            Description = "Freshly brewed arabica",
            Price = 3.50m,
            Stock = 120,
            Type = ItemType.Product
        },
        new Item
        {
            Name = "Buttermilk Pancakes",
            Description = "Stack of 3 with butter and syrup",
            Price = 7.50m,
            Stock = 60,
            Type = ItemType.Product
        }
    };

            var coffeeVariation = new ItemVariation
            {
                Name = "Size",
                Selection = ItemVariationSelection.Required,
                Item = sunriseItems[0]
            };
            var coffeeVariationOptions = new List<ItemVariationOption>
    {
        new ItemVariationOption { Name = "Small", PriceAdjustment = 0m, ItemVariation = coffeeVariation },
        new ItemVariationOption { Name = "Large", PriceAdjustment = 1.00m, ItemVariation = coffeeVariation }
    };

            var pancakeVariation = new ItemVariation
            {
                Name = "Toppings",
                Selection = ItemVariationSelection.Optional,
                Item = sunriseItems[1]
            };
            var pancakeVariationOptions = new List<ItemVariationOption>
    {
        new ItemVariationOption { Name = "Fresh Berries", PriceAdjustment = 2.00m, ItemVariation = pancakeVariation },
        new ItemVariationOption { Name = "Chocolate Chips", PriceAdjustment = 1.50m, ItemVariation = pancakeVariation }
    };

            var sunriseTax = new Tax
            {
                Name = "City VAT",
                Rate = 10.0m,
                CreatedAt = now,
                EffectiveFrom = now.AddDays(-30),
                Status = TaxStatus.Active
            };

            var sunriseDiscount = new Discount
            {
                Name = "Morning Promo",
                rate = 10m,
                ValidFrom = now.AddDays(-7),
                ValidTo = now.AddDays(7),
                Type = DiscountType.Percentage,
                AppliesTo = DiscountAppliesTo.Order,
                Status = DiscountStatus.Active
            };

            var sunriseServiceCharge = new ServiceCharge
            {
                Name = "Dine-in Service",
                rate = 5m,
                CreatedAt = now,
                UpdatedAt = now
            };

            var sunriseTables = new List<Table>
    {
        new Table { Name = "A1", Capacity = 4, Status = TableStatus.Free },
        new Table { Name = "A2", Capacity = 2, Status = TableStatus.Reserved }
    };

            var sunriseReservation = new Reservation
            {
                CustomerName = "John Doe",
                CustomerPhone = "+15559998888",
                CustomerNote = "Window seat, lactose free milk",
                AppointmentTime = now.AddHours(2),
                CreatedAt = now.AddHours(-1),
                Status = ReservationStatus.Scheduled,
                GuestNumber = 2,
                Table = sunriseTables[0]
            };

            var sunriseOrder = new Order
            {
                Tip = 2.00m,
                UpdatedAt = now,
                Status = OrderStatus.Closed,
                User = sunriseStaff,
                Table = sunriseTables[0],
                Discount = sunriseDiscount,
                Reservation = sunriseReservation
            };

            var sunriseOrderItems = new List<OrderItem>
    {
        new OrderItem
        {
            Order = sunriseOrder,
            Item = sunriseItems[0],
            Quantity = 2,
            Notes = "One large, one small",
            Tax = sunriseTax,
            ServiceCharge = sunriseServiceCharge,
            Discount = sunriseDiscount,
            Reservation = sunriseReservation
        },
        new OrderItem
        {
            Order = sunriseOrder,
            Item = sunriseItems[1],
            Quantity = 1,
            Notes = "Add berries",
            Tax = sunriseTax,
            ServiceCharge = sunriseServiceCharge,
            Discount = null,
            Reservation = sunriseReservation
        }
    };

            var sunrisePayment = new Payment
            {
                Order = sunriseOrder,
                Amount = 18.50m,
                PaidAt = now,
                Method = PaymentMethod.Card,
                Provider = PaymentProvider.ApplePay,
                Currency = PaymentCurrency.USD,
                Status = PaymentStatus.Completed
            };

            var sunriseRefund = new Refund
            {
                Order = sunriseOrder,
                Payment = sunrisePayment,
                amount = 3.50m,
                RefundedAt = now.AddMinutes(15),
                Reason = "Customer returned coffee",
                RefundMethod = PaymentMethod.Card,
                Currency = PaymentCurrency.USD,
                Status = PaymentStatus.Refunded
            };

            var sunriseGiftCard = new GiftCard
            {
                Code = "SUN123",
                InitialValue = 50m,
                CurrentBalance = 35m,
                Currency = PaymentCurrency.USD,
                IssuedAt = now.AddDays(-10),
                ExpiresAt = now.AddMonths(12),
                IssuedTo = "Jane Customer",
                Status = GiftCardStatus.Valid
            };

            // Business 2: Luna Spa (Beauty)
            var luna = new Business
            {
                Name = "Luna Spa",
                Phone = "+442080000111",
                Address = "77 High Street, London",
                Email = "hello@lunaspa.co.uk",
                Country = "UK",
                Currency = Currency.EUR,
                SubscriptionPlan = SubscriptionPlan.Beauty,
                WorkingHours = "09:00-21:00",
                NextPaymentDueDate = now.AddDays(25)
            };

            var lunaOwner = new User
            {
                Name = "Lucy Luna",
                Email = "lucy@lunaspa.co.uk",
                Phone = "+442080000222",
                Status = Status.Active
            };
            lunaOwner.PasswordHash = passwordHasher.HashPassword(lunaOwner, "owner123");

            var lunaTherapist = new User
            {
                Name = "Mina Stone",
                Email = "mina@lunaspa.co.uk",
                Phone = "+442080000333",
                Status = Status.Active
            };
            lunaTherapist.PasswordHash = passwordHasher.HashPassword(lunaTherapist, "therapist123");

            var lunaOwnerRole = new Role
            {
                Title = "Spa Owner",
                Description = "Owner with full permissions",
                CreatedAt = now,
                UpdatedAt = now,
                Status = Status.Active
            };

            var lunaTherapistRole = new Role
            {
                Title = "Therapist",
                Description = "Handles reservations and orders",
                CreatedAt = now,
                UpdatedAt = now,
                Status = Status.Active
            };

            var lunaRolePermissions = new List<RolePermission>
    {
        new RolePermission { Role = lunaOwnerRole, Permission = permissions[0], CreatedAt = now, UpdatedAt = now },
        new RolePermission { Role = lunaOwnerRole, Permission = permissions[1], CreatedAt = now, UpdatedAt = now },
        new RolePermission { Role = lunaOwnerRole, Permission = permissions[2], CreatedAt = now, UpdatedAt = now },
        new RolePermission { Role = lunaOwnerRole, Permission = permissions[3], CreatedAt = now, UpdatedAt = now },
        new RolePermission { Role = lunaTherapistRole, Permission = permissions[1], CreatedAt = now, UpdatedAt = now },
        new RolePermission { Role = lunaTherapistRole, Permission = permissions[3], CreatedAt = now, UpdatedAt = now }
    };

            var lunaUserRoles = new List<UserRole>
    {
        new UserRole { User = lunaOwner, Role = lunaOwnerRole, AssignedAt = now },
        new UserRole { User = lunaTherapist, Role = lunaTherapistRole, AssignedAt = now }
    };

            var lunaItems = new List<Item>
    {
        new Item
        {
            Name = "Deep Tissue Massage",
            Description = "60-minute full body massage",
            Price = 60m,
            Stock = 999,
            Type = ItemType.Service
        },
        new Item
        {
            Name = "Hydrating Facial",
            Description = "45-minute facial treatment",
            Price = 80m,
            Stock = 999,
            Type = ItemType.Service
        }
    };

            var massageVariation = new ItemVariation
            {
                Name = "Duration",
                Selection = ItemVariationSelection.Required,
                Item = lunaItems[0]
            };
            var massageVariationOptions = new List<ItemVariationOption>
    {
        new ItemVariationOption { Name = "60 min", PriceAdjustment = 0m, ItemVariation = massageVariation },
        new ItemVariationOption { Name = "90 min", PriceAdjustment = 25m, ItemVariation = massageVariation }
    };

            var lunaTax = new Tax
            {
                Name = "Service VAT",
                Rate = 15m,
                CreatedAt = now,
                EffectiveFrom = now.AddDays(-15),
                Status = TaxStatus.Active
            };

            var lunaDiscount = new Discount
            {
                Name = "New Client Offer",
                rate = 15m,
                ValidFrom = now.AddDays(-1),
                ValidTo = now.AddDays(30),
                Type = DiscountType.Percentage,
                AppliesTo = DiscountAppliesTo.Service,
                Status = DiscountStatus.Active
            };

            var lunaServiceCharge = new ServiceCharge
            {
                Name = "Treatment Fee",
                rate = 3m,
                CreatedAt = now,
                UpdatedAt = now
            };

            var lunaTables = new List<Table>
    {
        new Table { Name = "Room 1", Capacity = 1, Status = TableStatus.Free },
        new Table { Name = "Room 2", Capacity = 1, Status = TableStatus.Occupied }
    };

            var lunaReservation = new Reservation
            {
                CustomerName = "Alice Smith",
                CustomerPhone = "+447700900900",
                CustomerNote = "Prefers lavender oil",
                AppointmentTime = now.AddDays(1).AddHours(3),
                CreatedAt = now,
                Status = ReservationStatus.Scheduled,
                GuestNumber = 1,
                Table = lunaTables[0]
            };

            var lunaOrder = new Order
            {
                Tip = 5.00m,
                UpdatedAt = now,
                Status = OrderStatus.Closed,
                User = lunaTherapist,
                Table = lunaTables[0],
                Discount = lunaDiscount,
                Reservation = lunaReservation
            };

            var lunaOrderItems = new List<OrderItem>
    {
        new OrderItem
        {
            Order = lunaOrder,
            Item = lunaItems[0],
            Quantity = 1,
            Notes = "90 min session",
            Tax = lunaTax,
            ServiceCharge = lunaServiceCharge,
            Discount = lunaDiscount,
            Reservation = lunaReservation
        },
        new OrderItem
        {
            Order = lunaOrder,
            Item = lunaItems[1],
            Quantity = 1,
            Notes = "Add hydration mask",
            Tax = lunaTax,
            ServiceCharge = lunaServiceCharge,
            Discount = null,
            Reservation = lunaReservation
        }
    };

            var lunaPayment = new Payment
            {
                Order = lunaOrder,
                Amount = 120m,
                PaidAt = now.AddMinutes(10),
                Method = PaymentMethod.Card,
                Provider = PaymentProvider.Paypal,
                Currency = PaymentCurrency.EUR,
                Status = PaymentStatus.Completed
            };

            var lunaGiftCard = new GiftCard
            {
                Code = "LUNA999",
                InitialValue = 100m,
                CurrentBalance = 90m,
                Currency = PaymentCurrency.EUR,
                IssuedAt = now.AddDays(-5),
                ExpiresAt = now.AddMonths(18),
                IssuedTo = "Corporate Client",
                Status = GiftCardStatus.Valid
            };

            // -------------------------
            // 1) SEED PERMISSIONS (no FKs)
            // -------------------------
            await context.AddRangeAsync(permissions, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            // -------------------------
            // 2) SEED BUSINESSES FIRST -> get real Business.Id values
            // -------------------------
            await context.AddRangeAsync(new[] { sunrise, luna }, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            // -------------------------
            // 3) NOW SET ALL BusinessId / BussinessId that depend on Business.Id
            // -------------------------
            sunriseOwner.BusinessId = sunrise.Id;
            sunriseStaff.BusinessId = sunrise.Id;
            sunriseOwnerRole.BussinessId = sunrise.Id;
            sunriseStaffRole.BussinessId = sunrise.Id;

            sunriseItems.ForEach(i => i.BusinessId = sunrise.Id);
            sunriseTables.ForEach(t => t.BusinessId = sunrise.Id);
            sunriseTax.BusinessId = sunrise.Id;
            sunriseDiscount.BusinessId = sunrise.Id;
            sunriseServiceCharge.BusinessId = sunrise.Id;
            sunriseReservation.BusinessId = sunrise.Id;
            sunriseOrder.BusinessId = sunrise.Id;
            sunrisePayment.BussinesId = sunrise.Id;
            sunriseGiftCard.BusinessId = sunrise.Id;

            lunaOwner.BusinessId = luna.Id;
            lunaTherapist.BusinessId = luna.Id;
            lunaOwnerRole.BussinessId = luna.Id;
            lunaTherapistRole.BussinessId = luna.Id;

            lunaItems.ForEach(i => i.BusinessId = luna.Id);
            lunaTables.ForEach(t => t.BusinessId = luna.Id);
            lunaTax.BusinessId = luna.Id;
            lunaDiscount.BusinessId = luna.Id;
            lunaServiceCharge.BusinessId = luna.Id;
            lunaReservation.BusinessId = luna.Id;
            lunaOrder.BusinessId = luna.Id;
            lunaPayment.BussinesId = luna.Id;
            lunaGiftCard.BusinessId = luna.Id;

            // -------------------------
            // 4) SEED USERS + ROLES (now their BusinessId/BussinessId is valid)
            // -------------------------
            await context.AddRangeAsync(new object[]
            {
        sunriseOwner, sunriseStaff, lunaOwner, lunaTherapist,
        sunriseOwnerRole, sunriseStaffRole, lunaOwnerRole, lunaTherapistRole
            }, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            // -------------------------
            // 5) SET IDs THAT DEPEND ON User.Id (createdBy/assigned/issuedBy/ownerId/etc)
            // -------------------------
            sunrise.OwnerId = sunriseOwner.Id;
            sunriseReservation.CreatedBy = sunriseOwner.Id;
            sunriseReservation.AssignedEmployee = sunriseStaff.Id;
            sunriseRefund.UserId = sunriseOwner.Id;
            sunriseGiftCard.IssuedBy = sunriseOwner.Id;

            luna.OwnerId = lunaOwner.Id;
            lunaReservation.CreatedBy = lunaOwner.Id;
            lunaReservation.AssignedEmployee = lunaTherapist.Id;
            lunaGiftCard.IssuedBy = lunaOwner.Id;

            // -------------------------
            // 6) SEED EVERYTHING ELSE (all required FKs are now real)
            // -------------------------
            var remaining = new List<object>();

            remaining.AddRange(sunriseItems);
            remaining.AddRange(lunaItems);

            remaining.AddRange(new object[]
            {
        sunriseTax, sunriseDiscount, sunriseServiceCharge,
        lunaTax, lunaDiscount, lunaServiceCharge
            });

            remaining.AddRange(sunriseTables);
            remaining.AddRange(lunaTables);

            remaining.AddRange(new object[] { coffeeVariation, pancakeVariation, massageVariation });
            remaining.AddRange(coffeeVariationOptions);
            remaining.AddRange(pancakeVariationOptions);
            remaining.AddRange(massageVariationOptions);

            remaining.AddRange(new object[]
            {
        sunriseReservation, sunriseOrder, sunrisePayment, sunriseRefund, sunriseGiftCard,
        lunaReservation, lunaOrder, lunaPayment, lunaGiftCard
            });

            remaining.AddRange(sunriseRolePermissions);
            remaining.AddRange(lunaRolePermissions);
            remaining.AddRange(sunriseUserRoles);
            remaining.AddRange(lunaUserRoles);

            remaining.AddRange(sunriseOrderItems);
            remaining.AddRange(lunaOrderItems);

            await context.AddRangeAsync(remaining, cancellationToken);
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
