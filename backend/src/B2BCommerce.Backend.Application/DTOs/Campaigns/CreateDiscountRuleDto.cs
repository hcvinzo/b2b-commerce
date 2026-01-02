using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Campaigns;

/// <summary>
/// DTO for creating a new discount rule
/// </summary>
public class CreateDiscountRuleDto
{
    /// <summary>
    /// Type of discount (Percentage or FixedAmount)
    /// </summary>
    public DiscountType DiscountType { get; set; }

    /// <summary>
    /// Discount value - percentage (0-100) or fixed amount
    /// </summary>
    public decimal DiscountValue { get; set; }

    /// <summary>
    /// Maximum discount amount for percentage discounts (cap)
    /// </summary>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>
    /// How products are targeted
    /// </summary>
    public ProductTargetType ProductTargetType { get; set; }

    /// <summary>
    /// How customers are targeted
    /// </summary>
    public CustomerTargetType CustomerTargetType { get; set; }

    /// <summary>
    /// Minimum order amount to qualify (optional)
    /// </summary>
    public decimal? MinOrderAmount { get; set; }

    /// <summary>
    /// Minimum quantity to qualify (optional)
    /// </summary>
    public int? MinQuantity { get; set; }

    /// <summary>
    /// Product IDs when ProductTargetType is SpecificProducts
    /// </summary>
    public List<Guid>? ProductIds { get; set; }

    /// <summary>
    /// Category IDs when ProductTargetType is Categories
    /// </summary>
    public List<Guid>? CategoryIds { get; set; }

    /// <summary>
    /// Brand IDs when ProductTargetType is Brands
    /// </summary>
    public List<Guid>? BrandIds { get; set; }

    /// <summary>
    /// Customer IDs when CustomerTargetType is SpecificCustomers
    /// </summary>
    public List<Guid>? CustomerIds { get; set; }

    /// <summary>
    /// Price tiers when CustomerTargetType is CustomerTiers
    /// </summary>
    public List<PriceTier>? CustomerTiers { get; set; }
}
