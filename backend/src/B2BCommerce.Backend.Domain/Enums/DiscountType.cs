namespace B2BCommerce.Backend.Domain.Enums;

/// <summary>
/// Type of discount calculation
/// </summary>
public enum DiscountType
{
    /// <summary>
    /// Percentage discount (e.g., 10% off)
    /// </summary>
    Percentage = 1,

    /// <summary>
    /// Fixed amount discount (e.g., 50 TRY off)
    /// </summary>
    FixedAmount = 2
}
