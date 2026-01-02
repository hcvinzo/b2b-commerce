using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Campaigns;

/// <summary>
/// DTO for updating an existing discount rule
/// </summary>
public class UpdateDiscountRuleDto
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
}
