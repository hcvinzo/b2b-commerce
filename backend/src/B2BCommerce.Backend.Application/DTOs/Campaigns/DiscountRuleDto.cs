using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Campaigns;

/// <summary>
/// Discount rule data transfer object
/// </summary>
public class DiscountRuleDto
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public DiscountType DiscountType { get; set; }
    public string DiscountTypeName => DiscountType.ToString();
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public ProductTargetType ProductTargetType { get; set; }
    public string ProductTargetTypeName => ProductTargetType.ToString();
    public CustomerTargetType CustomerTargetType { get; set; }
    public string CustomerTargetTypeName => CustomerTargetType.ToString();
    public decimal? MinOrderAmount { get; set; }
    public int? MinQuantity { get; set; }

    // Targeting details
    public List<DiscountRuleProductDto> Products { get; set; } = new();
    public List<DiscountRuleCategoryDto> Categories { get; set; } = new();
    public List<DiscountRuleBrandDto> Brands { get; set; } = new();
    public List<DiscountRuleCustomerDto> Customers { get; set; } = new();
    public List<DiscountRuleCustomerTierDto> CustomerTiers { get; set; } = new();

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Product target in a discount rule
/// </summary>
public class DiscountRuleProductDto
{
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSku { get; set; }
}

/// <summary>
/// Category target in a discount rule
/// </summary>
public class DiscountRuleCategoryDto
{
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
}

/// <summary>
/// Brand target in a discount rule
/// </summary>
public class DiscountRuleBrandDto
{
    public Guid BrandId { get; set; }
    public string? BrandName { get; set; }
}

/// <summary>
/// Customer target in a discount rule
/// </summary>
public class DiscountRuleCustomerDto
{
    public Guid CustomerId { get; set; }
    public string? CustomerTitle { get; set; }
}

/// <summary>
/// Customer tier target in a discount rule
/// </summary>
public class DiscountRuleCustomerTierDto
{
    public PriceTier PriceTier { get; set; }
    public string PriceTierName => PriceTier.ToString();
}
