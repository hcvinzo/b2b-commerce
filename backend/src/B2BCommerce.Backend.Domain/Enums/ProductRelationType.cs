namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Types of relationships between products
/// </summary>
public enum ProductRelationType
{
    /// <summary>
    /// General related products (complementary or similar)
    /// </summary>
    Related = 1,

    /// <summary>
    /// Cross-sell products (frequently bought together)
    /// </summary>
    CrossSell = 2,

    /// <summary>
    /// Up-sell products (premium alternatives)
    /// </summary>
    UpSell = 3,

    /// <summary>
    /// Accessories for the product
    /// </summary>
    Accessories = 4
}
