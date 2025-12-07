namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Permission scopes for Integration API access control
/// </summary>
public static class IntegrationPermissionScopes
{
    // Product Scopes
    public const string ProductsRead = "products:read";
    public const string ProductsWrite = "products:write";

    // Stock Scopes
    public const string StockRead = "stock:read";
    public const string StockWrite = "stock:write";

    // Price Scopes
    public const string PricesRead = "prices:read";
    public const string PricesWrite = "prices:write";

    // Customer Scopes
    public const string CustomersRead = "customers:read";
    public const string CustomersWrite = "customers:write";

    // Order Scopes
    public const string OrdersRead = "orders:read";
    public const string OrdersWrite = "orders:write";

    // Invoice Scopes
    public const string InvoicesRead = "invoices:read";
    public const string InvoicesWrite = "invoices:write";

    // Webhook Management
    public const string WebhooksManage = "webhooks:manage";

    // Wildcards
    public const string All = "*";

    private static readonly HashSet<string> ValidScopes = new(StringComparer.OrdinalIgnoreCase)
    {
        ProductsRead, ProductsWrite,
        StockRead, StockWrite,
        PricesRead, PricesWrite,
        CustomersRead, CustomersWrite,
        OrdersRead, OrdersWrite,
        InvoicesRead, InvoicesWrite,
        WebhooksManage,
        All,
        "products:*", "stock:*", "prices:*",
        "customers:*", "orders:*", "invoices:*"
    };

    /// <summary>
    /// Validates if the given scope is a valid permission scope
    /// </summary>
    public static bool IsValidScope(string scope) => ValidScopes.Contains(scope);

    /// <summary>
    /// Gets all valid permission scopes
    /// </summary>
    public static IEnumerable<string> GetAllScopes() => ValidScopes;

    /// <summary>
    /// Gets read-only permission scopes
    /// </summary>
    public static IEnumerable<string> GetReadOnlyScopes() => new[]
    {
        ProductsRead, StockRead, PricesRead,
        CustomersRead, OrdersRead, InvoicesRead
    };

    /// <summary>
    /// Gets write permission scopes
    /// </summary>
    public static IEnumerable<string> GetWriteScopes() => new[]
    {
        ProductsWrite, StockWrite, PricesWrite,
        CustomersWrite, OrdersWrite, InvoicesWrite,
        WebhooksManage
    };
}
