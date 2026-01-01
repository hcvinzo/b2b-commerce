namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Permission scopes for Dealer Portal (Customer) access control
/// </summary>
public static class CustomerPermissionScopes
{
    /// <summary>
    /// Full access to all customer resources
    /// </summary>
    public const string All = "customer:*";

    // User Management (for customer admins)
    public const string UsersRead = "customer:users:read";
    public const string UsersWrite = "customer:users:write";

    // Orders
    public const string OrdersRead = "customer:orders:read";
    public const string OrdersCreate = "customer:orders:create";
    public const string OrdersEdit = "customer:orders:edit";

    // Account/Profile
    public const string ProfileRead = "customer:profile:read";
    public const string ProfileWrite = "customer:profile:write";

    // Addresses
    public const string AddressesRead = "customer:addresses:read";
    public const string AddressesWrite = "customer:addresses:write";

    // Catalog & Prices
    public const string CatalogRead = "customer:catalog:read";
    public const string PricesRead = "customer:prices:read";

    // Payments/Balance
    public const string BalanceRead = "customer:balance:read";

    /// <summary>
    /// Customer role names that cannot be deleted (system roles for customer user type)
    /// </summary>
    public static readonly string[] SystemCustomerRoles = { "CustomerAdmin", "CustomerPurchasing", "CustomerAccounting", "CustomerEmployee" };

    /// <summary>
    /// Protected customer role names that have special handling
    /// </summary>
    public static readonly string[] ProtectedCustomerRoles = { "CustomerAdmin" };

    /// <summary>
    /// Checks if a scope value is valid customer permission
    /// </summary>
    public static bool IsValidScope(string scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
            return false;

        if (scope == All)
            return true;

        return GetAllScopes().Contains(scope);
    }

    /// <summary>
    /// Gets all available customer permission scopes
    /// </summary>
    public static IEnumerable<string> GetAllScopes()
    {
        return GetCategories().SelectMany(c => c.Permissions.Select(p => p.Value));
    }

    /// <summary>
    /// Gets all customer permission categories with their scopes
    /// </summary>
    public static IEnumerable<PermissionCategory> GetCategories()
    {
        return new List<PermissionCategory>
        {
            new("User Management", "Manage dealer portal users", new[]
            {
                new PermissionScope(UsersRead, "View Users", "View users in the dealer account"),
                new PermissionScope(UsersWrite, "Manage Users", "Add, edit, and remove users in the dealer account")
            }),

            new("Orders", "Order management", new[]
            {
                new PermissionScope(OrdersRead, "View Orders", "View order list and details"),
                new PermissionScope(OrdersCreate, "Create Orders", "Place new orders"),
                new PermissionScope(OrdersEdit, "Edit Orders", "Modify pending orders")
            }),

            new("Profile", "Account profile management", new[]
            {
                new PermissionScope(ProfileRead, "View Profile", "View dealer account information"),
                new PermissionScope(ProfileWrite, "Edit Profile", "Update dealer account information")
            }),

            new("Addresses", "Address management", new[]
            {
                new PermissionScope(AddressesRead, "View Addresses", "View shipping and billing addresses"),
                new PermissionScope(AddressesWrite, "Manage Addresses", "Add, edit, and remove addresses")
            }),

            new("Catalog", "Product catalog access", new[]
            {
                new PermissionScope(CatalogRead, "Browse Catalog", "View products and categories"),
                new PermissionScope(PricesRead, "View Prices", "View product prices and discounts")
            }),

            new("Finance", "Financial information", new[]
            {
                new PermissionScope(BalanceRead, "View Balance", "View account balance and payment history")
            })
        };
    }
}
