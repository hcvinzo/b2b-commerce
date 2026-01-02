using B2BCommerce.Backend.Domain.Common;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Junction table for brands targeted by a discount rule
/// </summary>
public class DiscountRuleBrand : BaseEntity
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
    /// The targeted brand
    /// </summary>
    public Guid BrandId { get; protected set; }

    /// <summary>
    /// Navigation property to brand
    /// </summary>
    public Brand Brand { get; protected set; } = null!;

    private DiscountRuleBrand()
    {
    }

    /// <summary>
    /// Creates a new discount rule brand target
    /// </summary>
    public static DiscountRuleBrand Create(Guid discountRuleId, Guid brandId)
    {
        return new DiscountRuleBrand
        {
            DiscountRuleId = discountRuleId,
            BrandId = brandId
        };
    }
}
