namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Types of product collections
/// </summary>
public enum CollectionType
{
    /// <summary>
    /// Products are added manually by admin
    /// </summary>
    Manual = 1,

    /// <summary>
    /// Products are defined dynamically by filters
    /// </summary>
    Dynamic = 2
}
