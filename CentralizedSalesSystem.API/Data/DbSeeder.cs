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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CentralizedSalesSystem.API.Data
{
    public class DbSeeder
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public DbSeeder(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var scopedProvider = scope.ServiceProvider;
            var context = scopedProvider.GetRequiredService<CentralizedSalesDbContext>();
            var passwordHasher = scopedProvider.GetRequiredService<IPasswordHasher<User>>();
            var logger = scopedProvider.GetRequiredService<ILogger<CentralizedSalesDbContext>>();

            await MigrateWithRecoveryAsync(context, logger, cancellationToken);

            var now = DateTimeOffset.UtcNow;

            var superAdminEmails = (_configuration.GetSection("SuperAdmins").Get<SuperAdminSeed[]>() ?? Array.Empty<SuperAdminSeed>())
                .Select(sa => sa.Email?.Trim().ToLowerInvariant())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToHashSet();

            var hasNonSuperUsers = await context.Users
                .AnyAsync(u => !superAdminEmails.Contains(u.Email.ToLower()), cancellationToken);

            if (hasNonSuperUsers)
            {
                await EnsureSuperAdminsAsync(context, passwordHasher, cancellationToken);
                return;
            }

            await using var tx = await context.Database.BeginTransactionAsync(cancellationToken);

            // -------------------------
            // 0) CREATE IN-MEMORY OBJECTS
            // -------------------------

            var permissionDefinitions = new (PermissionCode code, string title, string description, PermissionResource resource)[]
            {
                (PermissionCode.MANAGE_ALL, "Manage All", "Full access to business resources", PermissionResource.Business),

                (PermissionCode.BUSINESS_VIEW, "View business", "View business profile and settings", PermissionResource.Business),
                (PermissionCode.BUSINESS_UPDATE, "Update business", "Edit business details", PermissionResource.Business),
                (PermissionCode.BUSINESS_DELETE, "Delete business", "Delete or deactivate business", PermissionResource.Business),
                (PermissionCode.BUSINESS_SUBSCRIPTION_MANAGE, "Manage subscription", "Change subscription plan or billing settings", PermissionResource.Business),

                (PermissionCode.USER_VIEW, "View users", "View users and staff profiles", PermissionResource.User),
                (PermissionCode.USER_CREATE, "Create user", "Add new staff or users", PermissionResource.User),
                (PermissionCode.USER_UPDATE, "Update user", "Edit user details", PermissionResource.User),
                (PermissionCode.USER_DELETE, "Delete user", "Deactivate or remove a user", PermissionResource.User),

                (PermissionCode.ROLE_VIEW, "View roles", "View roles and assigned permissions", PermissionResource.User),
                (PermissionCode.ROLE_CREATE, "Create role", "Create a new role", PermissionResource.User),
                (PermissionCode.ROLE_UPDATE, "Update role", "Edit an existing role", PermissionResource.User),
                (PermissionCode.ROLE_DELETE, "Delete role", "Delete or deactivate a role", PermissionResource.User),

                (PermissionCode.PERMISSION_VIEW, "View permissions", "View permissions list", PermissionResource.User),
                (PermissionCode.PERMISSION_ASSIGN, "Assign permissions", "Assign permissions to roles", PermissionResource.User),
                (PermissionCode.USER_ROLE_ASSIGN, "Assign roles", "Assign roles to users", PermissionResource.User),

                (PermissionCode.ITEM_VIEW, "View items", "View products or services", PermissionResource.Product),
                (PermissionCode.ITEM_CREATE, "Create item", "Create a new product or service", PermissionResource.Product),
                (PermissionCode.ITEM_UPDATE, "Update item", "Edit product or service details", PermissionResource.Product),
                (PermissionCode.ITEM_DELETE, "Delete item", "Deactivate or remove product or service", PermissionResource.Product),

                (PermissionCode.TAX_VIEW, "View tax", "View taxes", PermissionResource.Tax),
                (PermissionCode.TAX_MANAGE, "Manage tax", "Create, update, or deactivate tax rules", PermissionResource.Tax),

                (PermissionCode.SERVICE_CHARGE_VIEW, "View service charges", "View service charges", PermissionResource.ServiceCharge),
                (PermissionCode.SERVICE_CHARGE_MANAGE, "Manage service charges", "Create, update, or deactivate service charges", PermissionResource.ServiceCharge),

                (PermissionCode.DISCOUNT_VIEW, "View discounts", "View available discounts", PermissionResource.Discount),
                (PermissionCode.DISCOUNT_CREATE, "Create discount", "Create new discount", PermissionResource.Discount),
                (PermissionCode.DISCOUNT_UPDATE, "Update discount", "Modify discount", PermissionResource.Discount),
                (PermissionCode.DISCOUNT_DELETE, "Delete discount", "Deactivate or remove discount", PermissionResource.Discount),
                (PermissionCode.DISCOUNT_APPLY, "Apply discount", "Apply discount to an order item or an order", PermissionResource.Discount),

                (PermissionCode.ORDER_VIEW, "View orders", "View order list and details", PermissionResource.Order),
                (PermissionCode.ORDER_CREATE, "Create order", "Create new order", PermissionResource.Order),
                (PermissionCode.ORDER_UPDATE, "Update order", "Edit existing order", PermissionResource.Order),
                (PermissionCode.ORDER_DELETE, "Delete order", "Cancel or delete order", PermissionResource.Order),

                (PermissionCode.ORDER_ITEM_ADD, "Add order item", "Add item to order", PermissionResource.Order),
                (PermissionCode.ORDER_ITEM_UPDATE, "Update order item", "Modify order item", PermissionResource.Order),
                (PermissionCode.ORDER_ITEM_REMOVE, "Remove order item", "Remove order item", PermissionResource.Order),
                (PermissionCode.ORDER_CLOSE, "Close order", "Mark order as closed or paid", PermissionResource.Order),

                (PermissionCode.PAYMENT_VIEW, "View payments", "View payments and their status", PermissionResource.Payment),
                (PermissionCode.PAYMENT_CREATE, "Create payment", "Record or process a new payment", PermissionResource.Payment),
                (PermissionCode.PAYMENT_UPDATE, "Update payment", "Modify payment record", PermissionResource.Payment),
                (PermissionCode.PAYMENT_DELETE, "Delete payment", "Void or cancel payment", PermissionResource.Payment),
                (PermissionCode.PAYMENT_REFUND, "Refund payment", "Issue refund for a payment", PermissionResource.Payment),

                (PermissionCode.REFUND_VIEW, "View refunds", "View refund history", PermissionResource.Refund),
                (PermissionCode.REFUND_CREATE, "Create refund", "Create refund record", PermissionResource.Refund),
                (PermissionCode.REFUND_DELETE, "Delete refund", "Cancel or delete refund record", PermissionResource.Refund),

                (PermissionCode.GIFTCARD_ISSUE, "Issue gift card", "Issue a gift card", PermissionResource.GiftCard),
                (PermissionCode.GIFTCARD_REDEEM, "Redeem gift card", "Mark gift card as redeemed", PermissionResource.GiftCard),
                (PermissionCode.GIFTCARD_VOID, "Void gift card", "Void or cancel a gift card", PermissionResource.GiftCard),

                (PermissionCode.RESERVATION_VIEW, "View reservations", "View reservations or appointments", PermissionResource.Reservation),
                (PermissionCode.RESERVATION_CREATE, "Create reservation", "Add new reservation or appointment", PermissionResource.Reservation),
                (PermissionCode.RESERVATION_UPDATE, "Update reservation", "Modify existing reservation", PermissionResource.Reservation),
                (PermissionCode.RESERVATION_CANCEL, "Cancel reservation", "Cancel reservation", PermissionResource.Reservation),

                (PermissionCode.TABLE_VIEW, "View tables", "View tables or seating arrangements", PermissionResource.Table),
                (PermissionCode.TABLE_MANAGE, "Manage tables", "Create, update, or remove table configurations", PermissionResource.Table),
            };

            var permissions = permissionDefinitions
                .Select(p => new Permission
                {
                    Title = p.title,
                    Description = p.description,
                    Code = p.code.ToString(),
                    Resource = p.resource,
                    CreatedAt = now,
                    UpdatedAt = now,
                    Status = Status.Active
                })
                .ToList();

            var permByCode = permissions.ToDictionary(
                p => Enum.Parse<PermissionCode>(p.Code, ignoreCase: true),
                p => p);

            var ownerPermissionCodes = permByCode.Keys
                .Where(code => code != PermissionCode.MANAGE_ALL)
                .ToArray();

            List<RolePermission> BuildRolePermissions(Role role, IEnumerable<PermissionCode> codes)
            {
                return codes.Select(code => new RolePermission
                {
                    Role = role,
                    Permission = permByCode[code],
                    CreatedAt = now,
                    UpdatedAt = now
                }).ToList();
            }

            // -------------------------
            // BUSINESS 1: Sunrise Cafe
            // -------------------------
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
                Status = Status.Active,
                Business = sunrise
            };
            sunriseOwner.PasswordHash = passwordHasher.HashPassword(sunriseOwner, "owner123");

            var sunriseStaff = new User
            {
                Name = "Omar Waits",
                Email = "omar@sunrisecafe.com",
                Phone = "+15550003333",
                Status = Status.Active,
                Business = sunrise
            };
            sunriseStaff.PasswordHash = passwordHasher.HashPassword(sunriseStaff, "staff123");

            var sunriseOwnerRole = new Role
            {
                Title = "Cafe Owner",
                Description = "Owner with full permissions",
                BussinessId = 0, // set after Business save
                CreatedAt = now,
                UpdatedAt = now,
                Status = Status.Active
            };

            var sunriseStaffRole = new Role
            {
                Title = "Cafe Staff",
                Description = "Staff who manage orders and reservations",
                BussinessId = 0, // set after Business save
                CreatedAt = now,
                UpdatedAt = now,
                Status = Status.Active
            };

            var sunriseOwnerRolePermissions = BuildRolePermissions(
                sunriseOwnerRole,
                ownerPermissionCodes);

            var sunriseStaffRolePermissions = BuildRolePermissions(
                sunriseStaffRole,
                new[]
                {
                    PermissionCode.ORDER_VIEW, PermissionCode.ORDER_CREATE, PermissionCode.ORDER_UPDATE, PermissionCode.ORDER_DELETE,
                    PermissionCode.ORDER_ITEM_ADD, PermissionCode.ORDER_ITEM_UPDATE, PermissionCode.ORDER_ITEM_REMOVE, PermissionCode.ORDER_CLOSE,
                    PermissionCode.RESERVATION_VIEW, PermissionCode.RESERVATION_CREATE, PermissionCode.RESERVATION_UPDATE, PermissionCode.RESERVATION_CANCEL,
                    PermissionCode.ITEM_VIEW, PermissionCode.DISCOUNT_APPLY, PermissionCode.TABLE_VIEW
                });

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
                        CreatedBy = 0,          // set after user save
                        AssignedEmployee = 0,   // set after user save
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
                Status = PaymentStatus.Completed,
                BussinesId = 0 // set after business save
            };

            var sunriseRefund = new Refund
            {
                Order = sunriseOrder,
                Amount = 3.50m,
                RefundedAt = now.AddMinutes(15),
                Reason = "Customer returned coffee",
                RefundMethod = PaymentMethod.Card,
                Currency = PaymentCurrency.USD,
                Status = PaymentStatus.Refunded,
                UserId = 0 // set after user save
            };

            var sunriseGiftCard = new GiftCard
            {
                Code = "SUN123",
                InitialValue = 50m,
                CurrentBalance = 35m,
                Currency = PaymentCurrency.USD,
                IssuedAt = now.AddDays(-10),
                ExpiresAt = now.AddMonths(12),
                IssuedBy = 0, // set after user save
                IssuedTo = "Jane Customer",
                Status = GiftCardStatus.Valid
            };

            // -------------------------
            // BUSINESS 2: Luna Spa
            // -------------------------
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
                Status = Status.Active,
                Business = luna
            };
            lunaOwner.PasswordHash = passwordHasher.HashPassword(lunaOwner, "owner123");

            var lunaTherapist = new User
            {
                Name = "Mina Stone",
                Email = "mina@lunaspa.co.uk",
                Phone = "+442080000333",
                Status = Status.Active,
                Business = luna
            };
            lunaTherapist.PasswordHash = passwordHasher.HashPassword(lunaTherapist, "therapist123");

            var lunaOwnerRole = new Role
            {
                Title = "Spa Owner",
                Description = "Owner with full permissions",
                BussinessId = 0, // set after Business save
                CreatedAt = now,
                UpdatedAt = now,
                Status = Status.Active
            };

            var lunaTherapistRole = new Role
            {
                Title = "Therapist",
                Description = "Handles reservations and orders",
                BussinessId = 0, // set after Business save
                CreatedAt = now,
                UpdatedAt = now,
                Status = Status.Active
            };

            var lunaOwnerRolePermissions = BuildRolePermissions(
                lunaOwnerRole,
                ownerPermissionCodes);

            var lunaTherapistRolePermissions = BuildRolePermissions(
                lunaTherapistRole,
                new[]
                {
                    PermissionCode.ORDER_VIEW, PermissionCode.ORDER_CREATE, PermissionCode.ORDER_UPDATE, PermissionCode.ORDER_DELETE,
                    PermissionCode.ORDER_ITEM_ADD, PermissionCode.ORDER_ITEM_UPDATE, PermissionCode.ORDER_ITEM_REMOVE, PermissionCode.ORDER_CLOSE,
                    PermissionCode.RESERVATION_VIEW, PermissionCode.RESERVATION_CREATE, PermissionCode.RESERVATION_UPDATE, PermissionCode.RESERVATION_CANCEL,
                    PermissionCode.ITEM_VIEW, PermissionCode.DISCOUNT_APPLY, PermissionCode.TABLE_VIEW
                });

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
                        CreatedBy = 0,        // set after user save
                        AssignedEmployee = 0, // set after user save
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
                Status = PaymentStatus.Completed,
                BussinesId = 0 // set after business save
            };

            var lunaGiftCard = new GiftCard
            {
                Code = "LUNA999",
                InitialValue = 100m,
                CurrentBalance = 90m,
                Currency = PaymentCurrency.EUR,
                IssuedAt = now.AddDays(-5),
                ExpiresAt = now.AddMonths(18),
                IssuedBy = 0, // set after user save
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

            remaining.AddRange(sunriseOwnerRolePermissions);
            remaining.AddRange(sunriseStaffRolePermissions);
            remaining.AddRange(lunaOwnerRolePermissions);
            remaining.AddRange(lunaTherapistRolePermissions);

            remaining.AddRange(sunriseUserRoles);
            remaining.AddRange(lunaUserRoles);

            remaining.AddRange(sunriseOrderItems);
            remaining.AddRange(lunaOrderItems);

            await context.AddRangeAsync(remaining, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            await tx.CommitAsync(cancellationToken);

            await EnsureSuperAdminsAsync(context, passwordHasher, cancellationToken);

        }

        public async Task SeedSuperAdminsAsync(CancellationToken cancellationToken = default)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var scopedProvider = scope.ServiceProvider;
            var context = scopedProvider.GetRequiredService<CentralizedSalesDbContext>();
            var passwordHasher = scopedProvider.GetRequiredService<IPasswordHasher<User>>();
            var logger = scopedProvider.GetRequiredService<ILogger<CentralizedSalesDbContext>>();

            await MigrateWithRecoveryAsync(context, logger, cancellationToken);
            await EnsureSuperAdminsAsync(context, passwordHasher, cancellationToken);
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

        private async Task EnsureSuperAdminsAsync(
            CentralizedSalesDbContext context,
            IPasswordHasher<User> passwordHasher,
            CancellationToken cancellationToken)
        {
            var seeds = _configuration
                .GetSection("SuperAdmins")
                .Get<SuperAdminSeed[]>() ?? Array.Empty<SuperAdminSeed>();

            if (seeds.Length == 0)
            {
                seeds = new[]
                {
                    new SuperAdminSeed
                    {
                        Email = "super@ex.com",
                        Password = "Super123!",
                        Name = "Super Admin",
                        Phone = "+15550005555"
                    }
                };
            }

            var now = DateTimeOffset.UtcNow;

            var manageAll = await context.Permissions
                .FirstOrDefaultAsync(p => p.Code == PermissionCode.MANAGE_ALL.ToString(), cancellationToken);

            if (manageAll == null)
            {
                manageAll = new Permission
                {
                    Title = "Manage All",
                    Description = "Full access to business resources",
                    Code = PermissionCode.MANAGE_ALL.ToString(),
                    Resource = PermissionResource.Business,
                    CreatedAt = now,
                    UpdatedAt = now,
                    Status = Status.Active
                };
                context.Permissions.Add(manageAll);
                await context.SaveChangesAsync(cancellationToken);
            }

            var platformBusiness = await context.Businesses
                .FirstOrDefaultAsync(b => b.Email == "platform@system.local", cancellationToken);

            if (platformBusiness == null)
            {
                platformBusiness = new Business
                {
                    Name = "Platform",
                    Phone = "+10000000000",
                    Address = "Platform HQ",
                    Email = "platform@system.local",
                    Country = "N/A",
                    Currency = Currency.USD,
                    SubscriptionPlan = SubscriptionPlan.Catering
                };
                context.Businesses.Add(platformBusiness);
                await context.SaveChangesAsync(cancellationToken);
            }

            var superRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Title == "SuperAdmin" && r.BussinessId == platformBusiness.Id, cancellationToken);

            if (superRole == null)
            {
                superRole = new Role
                {
                    Title = "SuperAdmin",
                    Description = "Platform-wide super admin with full access",
                    BussinessId = platformBusiness.Id,
                    CreatedAt = now,
                    UpdatedAt = now,
                    Status = Status.Active
                };
                context.Roles.Add(superRole);
                await context.SaveChangesAsync(cancellationToken);
            }

            var hasRolePerm = await context.RolePermissions
                .AnyAsync(rp => rp.RoleID == superRole.Id && rp.PermissionID == manageAll.Id, cancellationToken);

            if (!hasRolePerm)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleID = superRole.Id,
                    PermissionID = manageAll.Id,
                    Role = superRole,
                    Permission = manageAll,
                    CreatedAt = now,
                    UpdatedAt = now
                });
                await context.SaveChangesAsync(cancellationToken);
            }

            foreach (var seed in seeds)
            {
                if (string.IsNullOrWhiteSpace(seed.Email)) continue;

                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Email == seed.Email, cancellationToken);

                if (user == null)
                {
                    user = new User
                    {
                        Name = string.IsNullOrWhiteSpace(seed.Name) ? seed.Email : seed.Name,
                        Email = seed.Email,
                        Phone = string.IsNullOrWhiteSpace(seed.Phone) ? "+10000000000" : seed.Phone,
                        Status = Status.Active,
                        BusinessId = platformBusiness.Id
                    };
                    user.PasswordHash = passwordHasher.HashPassword(user, seed.Password ?? "Super123!");
                    context.Users.Add(user);
                    await context.SaveChangesAsync(cancellationToken);
                }

                var hasUserRole = await context.UserRoles
                    .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == superRole.Id, cancellationToken);

                if (!hasUserRole)
                {
                    context.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = superRole.Id,
                        User = user,
                        Role = superRole,
                        AssignedAt = now
                    });
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private sealed class SuperAdminSeed
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = "Super123!";
            public string Name { get; set; } = "Super Admin";
            public string Phone { get; set; } = "+10000000000";
        }
    }
}
