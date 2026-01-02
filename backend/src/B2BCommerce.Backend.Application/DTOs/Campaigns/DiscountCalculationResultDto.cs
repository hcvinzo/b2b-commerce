using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Campaigns;

/// <summary>
/// Result of calculating the best discount for a product
/// </summary>
public class DiscountCalculationResultDto
{
    /// <summary>
    /// Campaign ID that provided the discount
    /// </summary>
    public Guid CampaignId { get; set; }

    /// <summary>
    /// Campaign name
    /// </summary>
    public string CampaignName { get; set; } = string.Empty;

    /// <summary>
    /// Discount rule ID that was applied
    /// </summary>
    public Guid DiscountRuleId { get; set; }

    /// <summary>
    /// Type of discount applied
    /// </summary>
    public DiscountType DiscountType { get; set; }

    /// <summary>
    /// Original discount value (percentage or fixed amount)
    /// </summary>
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// Calculated discount amount
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Currency of the discount
    /// </summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Original unit price
    /// </summary>
    public decimal OriginalUnitPrice { get; set; }

    /// <summary>
    /// Discounted unit price
    /// </summary>
    public decimal DiscountedUnitPrice { get; set; }

    /// <summary>
    /// Total original price (unit price * quantity)
    /// </summary>
    public decimal OriginalTotalPrice { get; set; }

    /// <summary>
    /// Total discounted price
    /// </summary>
    public decimal DiscountedTotalPrice { get; set; }

    /// <summary>
    /// Quantity for this calculation
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Discount percentage relative to original price
    /// </summary>
    public decimal DiscountPercentage => OriginalTotalPrice > 0
        ? Math.Round((DiscountAmount / OriginalTotalPrice) * 100, 2)
        : 0;
}

/// <summary>
/// Item for bulk discount calculation
/// </summary>
public class DiscountCalculationItemDto
{
    /// <summary>
    /// Product ID
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Unit price
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Currency
    /// </summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Quantity
    /// </summary>
    public int Quantity { get; set; }
}
