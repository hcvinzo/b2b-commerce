using B2BCommerce.Backend.Domain.Common;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Junction table for products targeted by a discount rule
/// </summary>
public class DiscountRuleProduct : BaseEntity
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
    /// The targeted product
    /// </summary>
    public Guid ProductId { get; protected set; }

    /// <summary>
    /// Navigation property to product
    /// </summary>
    public Product Product { get; protected set; } = null!;

    private DiscountRuleProduct()
    {
    }

    /// <summary>
    /// Creates a new discount rule product target
    /// </summary>
    public static DiscountRuleProduct Create(Guid discountRuleId, Guid productId)
    {
        return new DiscountRuleProduct
        {
            DiscountRuleId = discountRuleId,
            ProductId = productId
        };
    }
}
