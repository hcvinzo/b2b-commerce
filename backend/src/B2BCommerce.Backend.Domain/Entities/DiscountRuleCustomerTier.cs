using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Junction table for customer tiers targeted by a discount rule
/// </summary>
public class DiscountRuleCustomerTier : BaseEntity
{
    /// <summary>
    /// The discount rule this target belongs to
    /// </summary>
    public Guid DiscountRuleId { get; protected set; }

    /// <summary>
    /// Navigation property to discount rule
    /// </summary>
    public DiscountRule DiscountRule { get; protected set; } = null!;

    /// <summary>
    /// The targeted price tier
    /// </summary>
    public PriceTier PriceTier { get; protected set; }

    private DiscountRuleCustomerTier()
    {
    }

    /// <summary>
    /// Creates a new discount rule customer tier target
    /// </summary>
    public static DiscountRuleCustomerTier Create(Guid discountRuleId, PriceTier priceTier)
    {
        return new DiscountRuleCustomerTier
        {
            DiscountRuleId = discountRuleId,
            PriceTier = priceTier
        };
    }
}
