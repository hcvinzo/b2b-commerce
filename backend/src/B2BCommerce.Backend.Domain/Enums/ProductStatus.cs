namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a product
/// </summary>
public enum ProductStatus
{
    /// <summary>
    /// Product data is incomplete and requires editing before it can be activated.
    /// Products imported from external systems (ERP) without all required fields
    /// will be in this status.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Product is complete and available for display/sale.
    /// All required fields (Category, ProductType, ListPrice, TaxRate) must be set.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Product data is complete but has been intentionally disabled.
    /// Used for seasonal items, discontinued products, or temporary unavailability.
    /// </summary>
    Inactive = 2
}
