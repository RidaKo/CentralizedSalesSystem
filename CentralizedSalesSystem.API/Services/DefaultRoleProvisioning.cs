using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models;
using CentralizedSalesSystem.API.Models.Auth;
using CentralizedSalesSystem.API.Models.Auth.enums;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Services;

internal static class DefaultRoleProvisioning
{
    internal const string DefaultOwnerRoleTitle = "Owner";
    internal const string DefaultStaffRoleTitle = "Staff";
    private const string DefaultOwnerRoleDescription = "Default owner role with full business permissions";
    private const string DefaultStaffRoleDescription = "Default staff role for day-to-day operations";

    private sealed record PermissionDefinition(
        PermissionCode Code,
        string Title,
        string Description,
        PermissionResource Resource);

    private static readonly IReadOnlyDictionary<PermissionCode, PermissionDefinition> PermissionDefinitions =
        new Dictionary<PermissionCode, PermissionDefinition>
        {
            [PermissionCode.MANAGE_ALL] = new(
                PermissionCode.MANAGE_ALL,
                "Manage All",
                "Full access to business resources",
                PermissionResource.Business),

            [PermissionCode.BUSINESS_VIEW] = new(
                PermissionCode.BUSINESS_VIEW,
                "View business",
                "View business profile and settings",
                PermissionResource.Business),
            [PermissionCode.BUSINESS_UPDATE] = new(
                PermissionCode.BUSINESS_UPDATE,
                "Update business",
                "Edit business details",
                PermissionResource.Business),
            [PermissionCode.BUSINESS_DELETE] = new(
                PermissionCode.BUSINESS_DELETE,
                "Delete business",
                "Delete or deactivate business",
                PermissionResource.Business),
            [PermissionCode.BUSINESS_SUBSCRIPTION_MANAGE] = new(
                PermissionCode.BUSINESS_SUBSCRIPTION_MANAGE,
                "Manage subscription",
                "Change subscription plan or billing settings",
                PermissionResource.Business),

            [PermissionCode.USER_VIEW] = new(
                PermissionCode.USER_VIEW,
                "View users",
                "View users and staff profiles",
                PermissionResource.User),
            [PermissionCode.USER_CREATE] = new(
                PermissionCode.USER_CREATE,
                "Create user",
                "Add new staff or users",
                PermissionResource.User),
            [PermissionCode.USER_UPDATE] = new(
                PermissionCode.USER_UPDATE,
                "Update user",
                "Edit user details",
                PermissionResource.User),
            [PermissionCode.USER_DELETE] = new(
                PermissionCode.USER_DELETE,
                "Delete user",
                "Deactivate or remove a user",
                PermissionResource.User),

            [PermissionCode.ROLE_VIEW] = new(
                PermissionCode.ROLE_VIEW,
                "View roles",
                "View roles and assigned permissions",
                PermissionResource.User),
            [PermissionCode.ROLE_CREATE] = new(
                PermissionCode.ROLE_CREATE,
                "Create role",
                "Create a new role",
                PermissionResource.User),
            [PermissionCode.ROLE_UPDATE] = new(
                PermissionCode.ROLE_UPDATE,
                "Update role",
                "Edit an existing role",
                PermissionResource.User),
            [PermissionCode.ROLE_DELETE] = new(
                PermissionCode.ROLE_DELETE,
                "Delete role",
                "Delete or deactivate a role",
                PermissionResource.User),

            [PermissionCode.PERMISSION_VIEW] = new(
                PermissionCode.PERMISSION_VIEW,
                "View permissions",
                "View permissions list",
                PermissionResource.User),
            [PermissionCode.PERMISSION_ASSIGN] = new(
                PermissionCode.PERMISSION_ASSIGN,
                "Assign permissions",
                "Assign permissions to roles",
                PermissionResource.User),
            [PermissionCode.USER_ROLE_ASSIGN] = new(
                PermissionCode.USER_ROLE_ASSIGN,
                "Assign roles",
                "Assign roles to users",
                PermissionResource.User),

            [PermissionCode.ITEM_VIEW] = new(
                PermissionCode.ITEM_VIEW,
                "View items",
                "View products or services",
                PermissionResource.Product),
            [PermissionCode.ITEM_CREATE] = new(
                PermissionCode.ITEM_CREATE,
                "Create item",
                "Create a new product or service",
                PermissionResource.Product),
            [PermissionCode.ITEM_UPDATE] = new(
                PermissionCode.ITEM_UPDATE,
                "Update item",
                "Edit product or service details",
                PermissionResource.Product),
            [PermissionCode.ITEM_DELETE] = new(
                PermissionCode.ITEM_DELETE,
                "Delete item",
                "Deactivate or remove product or service",
                PermissionResource.Product),

            [PermissionCode.TAX_VIEW] = new(
                PermissionCode.TAX_VIEW,
                "View tax",
                "View taxes",
                PermissionResource.Tax),
            [PermissionCode.TAX_MANAGE] = new(
                PermissionCode.TAX_MANAGE,
                "Manage tax",
                "Create, update, or deactivate tax rules",
                PermissionResource.Tax),

            [PermissionCode.SERVICE_CHARGE_VIEW] = new(
                PermissionCode.SERVICE_CHARGE_VIEW,
                "View service charges",
                "View service charges",
                PermissionResource.ServiceCharge),
            [PermissionCode.SERVICE_CHARGE_MANAGE] = new(
                PermissionCode.SERVICE_CHARGE_MANAGE,
                "Manage service charges",
                "Create, update, or deactivate service charges",
                PermissionResource.ServiceCharge),

            [PermissionCode.DISCOUNT_VIEW] = new(
                PermissionCode.DISCOUNT_VIEW,
                "View discounts",
                "View available discounts",
                PermissionResource.Discount),
            [PermissionCode.DISCOUNT_CREATE] = new(
                PermissionCode.DISCOUNT_CREATE,
                "Create discount",
                "Create new discount",
                PermissionResource.Discount),
            [PermissionCode.DISCOUNT_UPDATE] = new(
                PermissionCode.DISCOUNT_UPDATE,
                "Update discount",
                "Modify discount",
                PermissionResource.Discount),
            [PermissionCode.DISCOUNT_DELETE] = new(
                PermissionCode.DISCOUNT_DELETE,
                "Delete discount",
                "Deactivate or remove discount",
                PermissionResource.Discount),
            [PermissionCode.DISCOUNT_APPLY] = new(
                PermissionCode.DISCOUNT_APPLY,
                "Apply discount",
                "Apply discount to an order item or an order",
                PermissionResource.Discount),

            [PermissionCode.ORDER_VIEW] = new(
                PermissionCode.ORDER_VIEW,
                "View orders",
                "View order list and details",
                PermissionResource.Order),
            [PermissionCode.ORDER_CREATE] = new(
                PermissionCode.ORDER_CREATE,
                "Create order",
                "Create new order",
                PermissionResource.Order),
            [PermissionCode.ORDER_UPDATE] = new(
                PermissionCode.ORDER_UPDATE,
                "Update order",
                "Edit existing order",
                PermissionResource.Order),
            [PermissionCode.ORDER_DELETE] = new(
                PermissionCode.ORDER_DELETE,
                "Delete order",
                "Cancel or delete order",
                PermissionResource.Order),

            [PermissionCode.ORDER_ITEM_ADD] = new(
                PermissionCode.ORDER_ITEM_ADD,
                "Add order item",
                "Add item to order",
                PermissionResource.Order),
            [PermissionCode.ORDER_ITEM_UPDATE] = new(
                PermissionCode.ORDER_ITEM_UPDATE,
                "Update order item",
                "Modify order item",
                PermissionResource.Order),
            [PermissionCode.ORDER_ITEM_REMOVE] = new(
                PermissionCode.ORDER_ITEM_REMOVE,
                "Remove order item",
                "Remove order item",
                PermissionResource.Order),
            [PermissionCode.ORDER_CLOSE] = new(
                PermissionCode.ORDER_CLOSE,
                "Close order",
                "Mark order as closed or paid",
                PermissionResource.Order),

            [PermissionCode.PAYMENT_VIEW] = new(
                PermissionCode.PAYMENT_VIEW,
                "View payments",
                "View payments and their status",
                PermissionResource.Payment),
            [PermissionCode.PAYMENT_CREATE] = new(
                PermissionCode.PAYMENT_CREATE,
                "Create payment",
                "Record or process a new payment",
                PermissionResource.Payment),
            [PermissionCode.PAYMENT_UPDATE] = new(
                PermissionCode.PAYMENT_UPDATE,
                "Update payment",
                "Modify payment record",
                PermissionResource.Payment),
            [PermissionCode.PAYMENT_DELETE] = new(
                PermissionCode.PAYMENT_DELETE,
                "Delete payment",
                "Void or cancel payment",
                PermissionResource.Payment),
            [PermissionCode.PAYMENT_REFUND] = new(
                PermissionCode.PAYMENT_REFUND,
                "Refund payment",
                "Issue refund for a payment",
                PermissionResource.Payment),

            [PermissionCode.REFUND_VIEW] = new(
                PermissionCode.REFUND_VIEW,
                "View refunds",
                "View refund history",
                PermissionResource.Refund),
            [PermissionCode.REFUND_CREATE] = new(
                PermissionCode.REFUND_CREATE,
                "Create refund",
                "Create refund record",
                PermissionResource.Refund),
            [PermissionCode.REFUND_DELETE] = new(
                PermissionCode.REFUND_DELETE,
                "Delete refund",
                "Cancel or delete refund record",
                PermissionResource.Refund),

            [PermissionCode.GIFTCARD_ISSUE] = new(
                PermissionCode.GIFTCARD_ISSUE,
                "Issue gift card",
                "Issue a gift card",
                PermissionResource.GiftCard),
            [PermissionCode.GIFTCARD_REDEEM] = new(
                PermissionCode.GIFTCARD_REDEEM,
                "Redeem gift card",
                "Mark gift card as redeemed",
                PermissionResource.GiftCard),
            [PermissionCode.GIFTCARD_VOID] = new(
                PermissionCode.GIFTCARD_VOID,
                "Void gift card",
                "Void or cancel a gift card",
                PermissionResource.GiftCard),

            [PermissionCode.RESERVATION_VIEW] = new(
                PermissionCode.RESERVATION_VIEW,
                "View reservations",
                "View reservations or appointments",
                PermissionResource.Reservation),
            [PermissionCode.RESERVATION_CREATE] = new(
                PermissionCode.RESERVATION_CREATE,
                "Create reservation",
                "Add new reservation or appointment",
                PermissionResource.Reservation),
            [PermissionCode.RESERVATION_UPDATE] = new(
                PermissionCode.RESERVATION_UPDATE,
                "Update reservation",
                "Modify existing reservation",
                PermissionResource.Reservation),
            [PermissionCode.RESERVATION_CANCEL] = new(
                PermissionCode.RESERVATION_CANCEL,
                "Cancel reservation",
                "Cancel reservation",
                PermissionResource.Reservation),

            [PermissionCode.TABLE_VIEW] = new(
                PermissionCode.TABLE_VIEW,
                "View tables",
                "View tables or seating arrangements",
                PermissionResource.Table),
            [PermissionCode.TABLE_MANAGE] = new(
                PermissionCode.TABLE_MANAGE,
                "Manage tables",
                "Create, update, or remove table configurations",
                PermissionResource.Table)
        };

    internal static readonly PermissionCode[] StaffPermissionCodes =
    {
        PermissionCode.ORDER_VIEW,
        PermissionCode.ORDER_CREATE,
        PermissionCode.ORDER_UPDATE,
        PermissionCode.ORDER_DELETE,
        PermissionCode.ORDER_ITEM_ADD,
        PermissionCode.ORDER_ITEM_UPDATE,
        PermissionCode.ORDER_ITEM_REMOVE,
        PermissionCode.ORDER_CLOSE,
        PermissionCode.RESERVATION_VIEW,
        PermissionCode.RESERVATION_CREATE,
        PermissionCode.RESERVATION_UPDATE,
        PermissionCode.RESERVATION_CANCEL,
        PermissionCode.ITEM_VIEW,
        PermissionCode.TAX_VIEW,
        PermissionCode.DISCOUNT_VIEW,
        PermissionCode.DISCOUNT_APPLY,
        PermissionCode.PAYMENT_VIEW,
        PermissionCode.PAYMENT_CREATE,
        PermissionCode.TABLE_VIEW
    };

    internal static IReadOnlyList<PermissionCode> OwnerPermissionCodes =>
        PermissionDefinitions.Keys
            .Where(code => code != PermissionCode.MANAGE_ALL)
            .ToList();

    internal sealed record DefaultRoles(Role OwnerRole, Role StaffRole);

    internal static async Task<DefaultRoles> EnsureDefaultRolesAsync(
        CentralizedSalesDbContext db,
        long businessId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var permissionCodes = OwnerPermissionCodes
            .Concat(StaffPermissionCodes)
            .Distinct()
            .ToArray();

        var permissionsByCode = await EnsurePermissionsAsync(db, permissionCodes, now, cancellationToken);

        var ownerRole = await EnsureRoleAsync(
            db,
            businessId,
            DefaultOwnerRoleTitle,
            DefaultOwnerRoleDescription,
            now,
            cancellationToken);

        await EnsureRolePermissionsAsync(
            db,
            ownerRole,
            OwnerPermissionCodes.Select(code => permissionsByCode[code]).ToList(),
            now,
            cancellationToken);

        var staffRole = await EnsureRoleAsync(
            db,
            businessId,
            DefaultStaffRoleTitle,
            DefaultStaffRoleDescription,
            now,
            cancellationToken);

        await EnsureRolePermissionsAsync(
            db,
            staffRole,
            StaffPermissionCodes.Select(code => permissionsByCode[code]).ToList(),
            now,
            cancellationToken);

        return new DefaultRoles(ownerRole, staffRole);
    }

    internal static async Task EnsureUserRoleAsync(
        CentralizedSalesDbContext db,
        User user,
        Role role,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var exists = await db.UserRoles
            .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, cancellationToken);

        if (exists)
        {
            return;
        }

        db.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            User = user,
            Role = role,
            AssignedAt = now
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Dictionary<PermissionCode, Permission>> EnsurePermissionsAsync(
        CentralizedSalesDbContext db,
        IEnumerable<PermissionCode> permissionCodes,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var codes = permissionCodes.Distinct().ToList();
        if (codes.Count == 0)
        {
            return new Dictionary<PermissionCode, Permission>();
        }

        var codeStrings = codes.Select(code => code.ToString()).ToList();
        var existingPermissions = await db.Permissions
            .Where(p => codeStrings.Contains(p.Code))
            .ToListAsync(cancellationToken);

        var permissionsByCode = existingPermissions
            .ToDictionary(p => Enum.Parse<PermissionCode>(p.Code, true), p => p);

        foreach (var code in codes)
        {
            if (permissionsByCode.ContainsKey(code))
            {
                continue;
            }

            if (!PermissionDefinitions.TryGetValue(code, out var definition))
            {
                definition = new PermissionDefinition(code, code.ToString(), code.ToString(), PermissionResource.Business);
            }

            var permission = new Permission
            {
                Title = definition.Title,
                Description = definition.Description,
                Code = definition.Code.ToString(),
                Resource = definition.Resource,
                CreatedAt = now,
                UpdatedAt = now,
                Status = Status.Active
            };

            db.Permissions.Add(permission);
            permissionsByCode[code] = permission;
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return permissionsByCode;
    }

    private static async Task<Role> EnsureRoleAsync(
        CentralizedSalesDbContext db,
        long businessId,
        string title,
        string description,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var role = await db.Roles
            .FirstOrDefaultAsync(r => r.BussinessId == businessId && r.Title == title, cancellationToken);

        if (role == null)
        {
            role = new Role
            {
                BussinessId = businessId,
                Title = title,
                Description = description,
                CreatedAt = now,
                UpdatedAt = now,
                Status = Status.Active
            };

            db.Roles.Add(role);
            await db.SaveChangesAsync(cancellationToken);
            return role;
        }

        if (role.Status != Status.Active)
        {
            role.Status = Status.Active;
            role.UpdatedAt = now;
            await db.SaveChangesAsync(cancellationToken);
        }

        return role;
    }

    private static async Task EnsureRolePermissionsAsync(
        CentralizedSalesDbContext db,
        Role role,
        IReadOnlyCollection<Permission> permissions,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (permissions.Count == 0)
        {
            return;
        }

        var permissionIds = permissions.Select(p => p.Id).ToList();
        var existingPermissionIds = await db.RolePermissions
            .Where(rp => rp.RoleID == role.Id && permissionIds.Contains(rp.PermissionID))
            .Select(rp => rp.PermissionID)
            .ToListAsync(cancellationToken);

        var missingPermissions = permissions
            .Where(p => !existingPermissionIds.Contains(p.Id))
            .ToList();

        if (missingPermissions.Count == 0)
        {
            return;
        }

        foreach (var permission in missingPermissions)
        {
            db.RolePermissions.Add(new RolePermission
            {
                RoleID = role.Id,
                PermissionID = permission.Id,
                Role = role,
                Permission = permission,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
