namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// How customers are targeted by a discount rule
/// </summary>
public enum CustomerTargetType
{
    /// <summary>
    /// Discount applies to all customers
    /// </summary>
    AllCustomers = 1,

    /// <summary>
    /// Discount applies to specific customers only
    /// </summary>
    SpecificCustomers = 2,

    /// <summary>
    /// Discount applies to customers in specific tiers
    /// </summary>
    CustomerTiers = 3
}
