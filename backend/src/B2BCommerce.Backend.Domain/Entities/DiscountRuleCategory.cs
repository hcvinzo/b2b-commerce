using B2BCommerce.Backend.Domain.Common;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Junction table for categories targeted by a discount rule
/// </summary>
public class DiscountRuleCategory : BaseEntity
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
    /// The targeted category
    /// </summary>
    public Guid CategoryId { get; protected set; }

    /// <summary>
    /// Navigation property to category
    /// </summary>
    public Category Category { get; protected set; } = null!;

    private DiscountRuleCategory()
    {
    }

    /// <summary>
    /// Creates a new discount rule category target
    /// </summary>
    public static DiscountRuleCategory Create(Guid discountRuleId, Guid categoryId)
    {
        return new DiscountRuleCategory
        {
            DiscountRuleId = discountRuleId,
            CategoryId = categoryId
        };
    }
}
