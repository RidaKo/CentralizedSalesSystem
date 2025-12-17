namespace CentralizedSalesSystem.API.Models.Auth.enums
{
    public enum PermissionCode
    {
        // Super permission
        MANAGE_ALL,

        // Business management
        BUSINESS_VIEW,
        BUSINESS_UPDATE,
        BUSINESS_DELETE,
        BUSINESS_SUBSCRIPTION_MANAGE,

        // User and staff management
        USER_VIEW,
        USER_CREATE,
        USER_UPDATE,
        USER_DELETE,
        ROLE_VIEW,
        ROLE_CREATE,
        ROLE_UPDATE,
        ROLE_DELETE,
        PERMISSION_VIEW,
        PERMISSION_ASSIGN,
        USER_ROLE_ASSIGN,

        // Product and service management
        ITEM_VIEW,
        ITEM_CREATE,
        ITEM_UPDATE,
        ITEM_DELETE,
        TAX_VIEW,
        TAX_MANAGE,
        SERVICE_CHARGE_VIEW,
        SERVICE_CHARGE_MANAGE,
        DISCOUNT_VIEW,
        DISCOUNT_CREATE,
        DISCOUNT_UPDATE,
        DISCOUNT_DELETE,
        DISCOUNT_APPLY,

        // Orders and order item management
        ORDER_VIEW,
        ORDER_CREATE,
        ORDER_UPDATE,
        ORDER_DELETE,
        ORDER_ITEM_ADD,
        ORDER_ITEM_UPDATE,
        ORDER_ITEM_REMOVE,
        ORDER_CLOSE,

        // Payment, refund and gift card management
        PAYMENT_VIEW,
        PAYMENT_CREATE,
        PAYMENT_UPDATE,
        PAYMENT_DELETE,
        PAYMENT_REFUND,
        REFUND_VIEW,
        REFUND_CREATE,
        REFUND_DELETE,
        GIFTCARD_ISSUE,
        GIFTCARD_REDEEM,
        GIFTCARD_VOID,

        // Reservation and table management
        RESERVATION_VIEW,
        RESERVATION_CREATE,
        RESERVATION_UPDATE,
        RESERVATION_CANCEL,
        TABLE_VIEW,
        TABLE_MANAGE
    }
}
