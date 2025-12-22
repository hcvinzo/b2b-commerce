namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Types of customer addresses
/// </summary>
public enum CustomerAddressType
{
    /// <summary>
    /// Billing address for invoices
    /// </summary>
    Billing = 1,

    /// <summary>
    /// Shipping address for deliveries
    /// </summary>
    Shipping = 2,

    /// <summary>
    /// Contact address for correspondence
    /// </summary>
    Contact = 3
}
