namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// How products are targeted by a discount rule
/// </summary>
public enum ProductTargetType
{
    /// <summary>
    /// Discount applies to all products
    /// </summary>
    AllProducts = 1,

    /// <summary>
    /// Discount applies to specific products only
    /// </summary>
    SpecificProducts = 2,

    /// <summary>
    /// Discount applies to products in specific categories (including subcategories)
    /// </summary>
    Categories = 3,

    /// <summary>
    /// Discount applies to products of specific brands
    /// </summary>
    Brands = 4
}
