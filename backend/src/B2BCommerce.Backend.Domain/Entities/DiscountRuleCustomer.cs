using B2BCommerce.Backend.Domain.Common;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Junction table for customers targeted by a discount rule
/// </summary>
public class DiscountRuleCustomer : BaseEntity
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
    /// The targeted customer
    /// </summary>
    public Guid CustomerId { get; protected set; }

    /// <summary>
    /// Navigation property to customer
    /// </summary>
    public Customer Customer { get; protected set; } = null!;

    private DiscountRuleCustomer()
    {
    }

    /// <summary>
    /// Creates a new discount rule customer target
    /// </summary>
    public static DiscountRuleCustomer Create(Guid discountRuleId, Guid customerId)
    {
        return new DiscountRuleCustomer
        {
            DiscountRuleId = discountRuleId,
            CustomerId = customerId
        };
    }
}
