using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Records usage of a campaign discount
/// </summary>
public class CampaignUsage : BaseEntity
{
    /// <summary>
    /// The campaign that was used
    /// </summary>
    public Guid CampaignId { get; protected set; }

    /// <summary>
    /// Navigation property to campaign
    /// </summary>
    public Campaign Campaign { get; protected set; } = null!;

    /// <summary>
    /// The customer who used the discount
    /// </summary>
    public Guid CustomerId { get; protected set; }

    /// <summary>
    /// Navigation property to customer
    /// </summary>
    public Customer Customer { get; protected set; } = null!;

    /// <summary>
    /// The order where the discount was applied
    /// </summary>
    public Guid OrderId { get; protected set; }

    /// <summary>
    /// Navigation property to order
    /// </summary>
    public Order Order { get; protected set; } = null!;

    /// <summary>
    /// The specific order item where the discount was applied (optional)
    /// </summary>
    public Guid? OrderItemId { get; protected set; }

    /// <summary>
    /// Navigation property to order item
    /// </summary>
    public OrderItem? OrderItem { get; protected set; }

    /// <summary>
    /// The discount amount applied
    /// </summary>
    public Money DiscountAmount { get; protected set; } = null!;

    /// <summary>
    /// When the discount was used
    /// </summary>
    public DateTime UsedAt { get; protected set; }

    /// <summary>
    /// Whether this usage has been reversed (e.g., order cancelled)
    /// </summary>
    public bool IsReversed { get; protected set; }

    /// <summary>
    /// When the usage was reversed
    /// </summary>
    public DateTime? ReversedAt { get; protected set; }

    private CampaignUsage()
    {
    }

    /// <summary>
    /// Creates a new campaign usage record
    /// </summary>
    public static CampaignUsage Create(
        Guid campaignId,
        Guid customerId,
        Guid orderId,
        Money discountAmount,
        Guid? orderItemId = null)
    {
        return new CampaignUsage
        {
            CampaignId = campaignId,
            CustomerId = customerId,
            OrderId = orderId,
            OrderItemId = orderItemId,
            DiscountAmount = discountAmount,
            UsedAt = DateTime.UtcNow,
            IsReversed = false
        };
    }

    /// <summary>
    /// Reverses this usage record
    /// </summary>
    public void Reverse()
    {
        if (IsReversed)
        {
            return; // Already reversed
        }

        IsReversed = true;
        ReversedAt = DateTime.UtcNow;
    }
}
