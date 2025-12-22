namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Permission scopes for Admin Panel access control
/// </summary>
public static class AdminPermissionScopes
{
    /// <summary>
    /// Full access to all resources
    /// </summary>
    public const string All = "*";

    // Dashboard
    public const string DashboardRead = "dashboard:read";

    // Products
    public const string ProductsRead = "products:read";
    public const string ProductsWrite = "products:write";
    public const string ProductsDelete = "products:delete";

    // Categories
    public const string CategoriesRead = "categories:read";
    public const string CategoriesWrite = "categories:write";
    public const string CategoriesDelete = "categories:delete";

    // Brands
    public const string BrandsRead = "brands:read";
    public const string BrandsWrite = "brands:write";
    public const string BrandsDelete = "brands:delete";

    // Attributes
    public const string AttributesRead = "attributes:read";
    public const string AttributesWrite = "attributes:write";
    public const string AttributesDelete = "attributes:delete";

    // Product Types
    public const string ProductTypesRead = "product-types:read";
    public const string ProductTypesWrite = "product-types:write";
    public const string ProductTypesDelete = "product-types:delete";

    // Orders
    public const string OrdersRead = "orders:read";
    public const string OrdersWrite = "orders:write";
    public const string OrdersProcess = "orders:process";
    public const string OrdersCancel = "orders:cancel";

    // Customers
    public const string CustomersRead = "customers:read";
    public const string CustomersWrite = "customers:write";
    public const string CustomersApprove = "customers:approve";
    public const string CustomersCredit = "customers:credit";

    // Users (Customer Portal Users)
    public const string UsersRead = "users:read";
    public const string UsersWrite = "users:write";

    // Admin Users
    public const string AdminUsersRead = "admin-users:read";
    public const string AdminUsersWrite = "admin-users:write";

    // Roles
    public const string RolesRead = "roles:read";
    public const string RolesWrite = "roles:write";

    // API Clients
    public const string ApiClientsRead = "api-clients:read";
    public const string ApiClientsWrite = "api-clients:write";

    // Settings
    public const string SettingsRead = "settings:read";
    public const string SettingsWrite = "settings:write";

    // Reports
    public const string ReportsRead = "reports:read";
    public const string ReportsExport = "reports:export";

    /// <summary>
    /// System role names that cannot be deleted
    /// </summary>
    public static readonly string[] SystemRoles = { "Admin", "SalesRep", "Customer" };

    /// <summary>
    /// Protected role names that have special handling
    /// </summary>
    public static readonly string[] ProtectedRoles = { "Admin" };

    /// <summary>
    /// Checks if a scope value is valid
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
    /// Gets all available permission scopes
    /// </summary>
    public static IEnumerable<string> GetAllScopes()
    {
        return GetCategories().SelectMany(c => c.Permissions.Select(p => p.Value));
    }

    /// <summary>
    /// Gets all permission categories with their scopes
    /// </summary>
    public static IEnumerable<PermissionCategory> GetCategories()
    {
        return new List<PermissionCategory>
        {
            new("Dashboard", "Dashboard access", new[]
            {
                new PermissionScope(DashboardRead, "View Dashboard", "Access to view dashboard and analytics")
            }),

            new("Products", "Product management", new[]
            {
                new PermissionScope(ProductsRead, "View Products", "Access to view product list and details"),
                new PermissionScope(ProductsWrite, "Edit Products", "Create and update products"),
                new PermissionScope(ProductsDelete, "Delete Products", "Delete products from the system")
            }),

            new("Categories", "Category management", new[]
            {
                new PermissionScope(CategoriesRead, "View Categories", "Access to view category tree and details"),
                new PermissionScope(CategoriesWrite, "Edit Categories", "Create and update categories"),
                new PermissionScope(CategoriesDelete, "Delete Categories", "Delete categories from the system")
            }),

            new("Brands", "Brand management", new[]
            {
                new PermissionScope(BrandsRead, "View Brands", "Access to view brand list and details"),
                new PermissionScope(BrandsWrite, "Edit Brands", "Create and update brands"),
                new PermissionScope(BrandsDelete, "Delete Brands", "Delete brands from the system")
            }),

            new("Attributes", "Attribute management", new[]
            {
                new PermissionScope(AttributesRead, "View Attributes", "Access to view attribute definitions"),
                new PermissionScope(AttributesWrite, "Edit Attributes", "Create and update attributes"),
                new PermissionScope(AttributesDelete, "Delete Attributes", "Delete attributes from the system")
            }),

            new("Product Types", "Product type management", new[]
            {
                new PermissionScope(ProductTypesRead, "View Product Types", "Access to view product types"),
                new PermissionScope(ProductTypesWrite, "Edit Product Types", "Create and update product types"),
                new PermissionScope(ProductTypesDelete, "Delete Product Types", "Delete product types from the system")
            }),

            new("Orders", "Order management", new[]
            {
                new PermissionScope(OrdersRead, "View Orders", "Access to view order list and details"),
                new PermissionScope(OrdersWrite, "Edit Orders", "Update order information"),
                new PermissionScope(OrdersProcess, "Process Orders", "Process and fulfill orders"),
                new PermissionScope(OrdersCancel, "Cancel Orders", "Cancel orders")
            }),

            new("Customers", "Customer management", new[]
            {
                new PermissionScope(CustomersRead, "View Customers", "Access to view customer list and details"),
                new PermissionScope(CustomersWrite, "Edit Customers", "Create and update customer information"),
                new PermissionScope(CustomersApprove, "Approve Customers", "Approve or reject customer registrations"),
                new PermissionScope(CustomersCredit, "Manage Credit", "Manage customer credit limits")
            }),

            new("Users", "Customer portal user management", new[]
            {
                new PermissionScope(UsersRead, "View Users", "Access to view customer portal users"),
                new PermissionScope(UsersWrite, "Edit Users", "Create and update customer portal users")
            }),

            new("Admin Users", "Admin panel user management", new[]
            {
                new PermissionScope(AdminUsersRead, "View Admin Users", "Access to view admin user list"),
                new PermissionScope(AdminUsersWrite, "Edit Admin Users", "Create and update admin users")
            }),

            new("Roles", "Role and permission management", new[]
            {
                new PermissionScope(RolesRead, "View Roles", "Access to view roles and permissions"),
                new PermissionScope(RolesWrite, "Edit Roles", "Create, update, and delete roles")
            }),

            new("API Clients", "API client management", new[]
            {
                new PermissionScope(ApiClientsRead, "View API Clients", "Access to view API clients"),
                new PermissionScope(ApiClientsWrite, "Edit API Clients", "Create and update API clients")
            }),

            new("Settings", "System settings", new[]
            {
                new PermissionScope(SettingsRead, "View Settings", "Access to view system settings"),
                new PermissionScope(SettingsWrite, "Edit Settings", "Modify system settings")
            }),

            new("Reports", "Reporting and analytics", new[]
            {
                new PermissionScope(ReportsRead, "View Reports", "Access to view reports and analytics"),
                new PermissionScope(ReportsExport, "Export Reports", "Export reports to files")
            })
        };
    }
}

/// <summary>
/// Represents a category of permissions
/// </summary>
public record PermissionCategory(
    string Name,
    string Description,
    IEnumerable<PermissionScope> Permissions);

/// <summary>
/// Represents a single permission scope
/// </summary>
public record PermissionScope(
    string Value,
    string DisplayName,
    string Description);
