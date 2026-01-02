using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Discount rule defining how discounts are calculated and applied.
/// A campaign can have multiple discount rules.
/// </summary>
public class DiscountRule : BaseEntity
{
    /// <summary>
    /// The campaign this rule belongs to
    /// </summary>
    public Guid CampaignId { get; protected set; }

    /// <summary>
    /// Navigation property to campaign
    /// </summary>
    public Campaign Campaign { get; protected set; } = null!;

    /// <summary>
    /// Type of discount (Percentage or FixedAmount)
    /// </summary>
    public DiscountType DiscountType { get; protected set; }

    /// <summary>
    /// Discount value - percentage (0-100) or fixed amount
    /// </summary>
    public decimal DiscountValue { get; protected set; }

    /// <summary>
    /// Maximum discount amount for percentage discounts (cap)
    /// </summary>
    public decimal? MaxDiscountAmount { get; protected set; }

    /// <summary>
    /// How products are targeted
    /// </summary>
    public ProductTargetType ProductTargetType { get; protected set; }

    /// <summary>
    /// How customers are targeted
    /// </summary>
    public CustomerTargetType CustomerTargetType { get; protected set; }

    /// <summary>
    /// Minimum order amount to qualify for this discount (optional)
    /// </summary>
    public decimal? MinOrderAmount { get; protected set; }

    /// <summary>
    /// Minimum quantity to qualify for this discount (optional)
    /// </summary>
    public int? MinQuantity { get; protected set; }

    /// <summary>
    /// Targeted products (when ProductTargetType = SpecificProducts)
    /// </summary>
    public ICollection<DiscountRuleProduct> Products { get; protected set; } = new List<DiscountRuleProduct>();

    /// <summary>
    /// Targeted categories (when ProductTargetType = Categories)
    /// </summary>
    public ICollection<DiscountRuleCategory> Categories { get; protected set; } = new List<DiscountRuleCategory>();

    /// <summary>
    /// Targeted brands (when ProductTargetType = Brands)
    /// </summary>
    public ICollection<DiscountRuleBrand> Brands { get; protected set; } = new List<DiscountRuleBrand>();

    /// <summary>
    /// Targeted customers (when CustomerTargetType = SpecificCustomers)
    /// </summary>
    public ICollection<DiscountRuleCustomer> Customers { get; protected set; } = new List<DiscountRuleCustomer>();

    /// <summary>
    /// Targeted customer tiers (when CustomerTargetType = CustomerTiers)
    /// </summary>
    public ICollection<DiscountRuleCustomerTier> CustomerTiers { get; protected set; } = new List<DiscountRuleCustomerTier>();

    private DiscountRule()
    {
    }

    /// <summary>
    /// Creates a new discount rule
    /// </summary>
    public static DiscountRule Create(
        Guid campaignId,
        DiscountType discountType,
        decimal discountValue,
        ProductTargetType productTargetType,
        CustomerTargetType customerTargetType,
        decimal? maxDiscountAmount = null,
        decimal? minOrderAmount = null,
        int? minQuantity = null)
    {
        ValidateDiscountValue(discountType, discountValue);

        var rule = new DiscountRule
        {
            CampaignId = campaignId,
            DiscountType = discountType,
            DiscountValue = discountValue,
            MaxDiscountAmount = maxDiscountAmount,
            ProductTargetType = productTargetType,
            CustomerTargetType = customerTargetType,
            MinOrderAmount = minOrderAmount,
            MinQuantity = minQuantity
        };

        return rule;
    }

    /// <summary>
    /// Updates the discount rule
    /// </summary>
    public void Update(
        DiscountType discountType,
        decimal discountValue,
        ProductTargetType productTargetType,
        CustomerTargetType customerTargetType,
        decimal? maxDiscountAmount = null,
        decimal? minOrderAmount = null,
        int? minQuantity = null)
    {
        ValidateDiscountValue(discountType, discountValue);

        DiscountType = discountType;
        DiscountValue = discountValue;
        MaxDiscountAmount = maxDiscountAmount;
        ProductTargetType = productTargetType;
        CustomerTargetType = customerTargetType;
        MinOrderAmount = minOrderAmount;
        MinQuantity = minQuantity;
    }

    /// <summary>
    /// Calculates the discount amount for a given price and quantity
    /// </summary>
    public Money CalculateDiscount(Money unitPrice, int quantity, string currency = "TRY")
    {
        // Check minimum quantity requirement
        if (MinQuantity.HasValue && quantity < MinQuantity.Value)
        {
            return Money.Zero(currency);
        }

        var totalPrice = unitPrice.Amount * quantity;

        // Check minimum order amount requirement
        if (MinOrderAmount.HasValue && totalPrice < MinOrderAmount.Value)
        {
            return Money.Zero(currency);
        }

        decimal discountAmount;

        switch (DiscountType)
        {
            case Enums.DiscountType.Percentage:
                discountAmount = totalPrice * (DiscountValue / 100);
                // Apply cap if specified
                if (MaxDiscountAmount.HasValue && discountAmount > MaxDiscountAmount.Value)
                {
                    discountAmount = MaxDiscountAmount.Value;
                }
                break;

            case Enums.DiscountType.FixedAmount:
                // Fixed amount per unit
                discountAmount = DiscountValue * quantity;
                // Cannot discount more than total price
                if (discountAmount > totalPrice)
                {
                    discountAmount = totalPrice;
                }
                break;

            default:
                discountAmount = 0;
                break;
        }

        return new Money(discountAmount, currency);
    }

    /// <summary>
    /// Checks if this rule applies to the given product
    /// </summary>
    public bool AppliesToProduct(Guid productId, Guid? categoryId, IEnumerable<Guid> categoryAncestorIds, Guid? brandId)
    {
        switch (ProductTargetType)
        {
            case Enums.ProductTargetType.AllProducts:
                return true;

            case Enums.ProductTargetType.SpecificProducts:
                return Products.Any(p => p.ProductId == productId);

            case Enums.ProductTargetType.Categories:
                if (!categoryId.HasValue)
                {
                    return false;
                }
                // Check if product's category or any of its ancestors is in the rule's categories
                var targetCategoryIds = Categories.Select(c => c.CategoryId).ToHashSet();
                if (targetCategoryIds.Contains(categoryId.Value))
                {
                    return true;
                }
                return categoryAncestorIds.Any(ancestorId => targetCategoryIds.Contains(ancestorId));

            case Enums.ProductTargetType.Brands:
                if (!brandId.HasValue)
                {
                    return false;
                }
                return Brands.Any(b => b.BrandId == brandId.Value);

            default:
                return false;
        }
    }

    /// <summary>
    /// Checks if this rule applies to the given customer
    /// </summary>
    public bool AppliesToCustomer(Guid customerId, PriceTier customerTier)
    {
        switch (CustomerTargetType)
        {
            case Enums.CustomerTargetType.AllCustomers:
                return true;

            case Enums.CustomerTargetType.SpecificCustomers:
                return Customers.Any(c => c.CustomerId == customerId);

            case Enums.CustomerTargetType.CustomerTiers:
                return CustomerTiers.Any(t => t.PriceTier == customerTier);

            default:
                return false;
        }
    }

    /// <summary>
    /// Adds a product to the rule's targeted products
    /// </summary>
    public void AddProduct(Guid productId)
    {
        if (ProductTargetType != Enums.ProductTargetType.SpecificProducts)
        {
            throw new InvalidOperationDomainException("Can only add products when ProductTargetType is SpecificProducts");
        }

        if (Products.Any(p => p.ProductId == productId))
        {
            return; // Already added
        }

        Products.Add(DiscountRuleProduct.Create(Id, productId));
    }

    /// <summary>
    /// Adds a category to the rule's targeted categories
    /// </summary>
    public void AddCategory(Guid categoryId)
    {
        if (ProductTargetType != Enums.ProductTargetType.Categories)
        {
            throw new InvalidOperationDomainException("Can only add categories when ProductTargetType is Categories");
        }

        if (Categories.Any(c => c.CategoryId == categoryId))
        {
            return; // Already added
        }

        Categories.Add(DiscountRuleCategory.Create(Id, categoryId));
    }

    /// <summary>
    /// Adds a brand to the rule's targeted brands
    /// </summary>
    public void AddBrand(Guid brandId)
    {
        if (ProductTargetType != Enums.ProductTargetType.Brands)
        {
            throw new InvalidOperationDomainException("Can only add brands when ProductTargetType is Brands");
        }

        if (Brands.Any(b => b.BrandId == brandId))
        {
            return; // Already added
        }

        Brands.Add(DiscountRuleBrand.Create(Id, brandId));
    }

    /// <summary>
    /// Adds a customer to the rule's targeted customers
    /// </summary>
    public void AddCustomer(Guid customerId)
    {
        if (CustomerTargetType != Enums.CustomerTargetType.SpecificCustomers)
        {
            throw new InvalidOperationDomainException("Can only add customers when CustomerTargetType is SpecificCustomers");
        }

        if (Customers.Any(c => c.CustomerId == customerId))
        {
            return; // Already added
        }

        Customers.Add(DiscountRuleCustomer.Create(Id, customerId));
    }

    /// <summary>
    /// Adds a customer tier to the rule's targeted tiers
    /// </summary>
    public void AddCustomerTier(PriceTier tier)
    {
        if (CustomerTargetType != Enums.CustomerTargetType.CustomerTiers)
        {
            throw new InvalidOperationDomainException("Can only add tiers when CustomerTargetType is CustomerTiers");
        }

        if (CustomerTiers.Any(t => t.PriceTier == tier))
        {
            return; // Already added
        }

        CustomerTiers.Add(DiscountRuleCustomerTier.Create(Id, tier));
    }

    /// <summary>
    /// Clears all product targets
    /// </summary>
    public void ClearProducts()
    {
        Products.Clear();
    }

    /// <summary>
    /// Clears all category targets
    /// </summary>
    public void ClearCategories()
    {
        Categories.Clear();
    }

    /// <summary>
    /// Clears all brand targets
    /// </summary>
    public void ClearBrands()
    {
        Brands.Clear();
    }

    /// <summary>
    /// Clears all customer targets
    /// </summary>
    public void ClearCustomers()
    {
        Customers.Clear();
    }

    /// <summary>
    /// Clears all customer tier targets
    /// </summary>
    public void ClearCustomerTiers()
    {
        CustomerTiers.Clear();
    }

    private static void ValidateDiscountValue(DiscountType discountType, decimal discountValue)
    {
        if (discountValue <= 0)
        {
            throw new DomainException("Discount value must be greater than 0");
        }

        if (discountType == Enums.DiscountType.Percentage && discountValue > 100)
        {
            throw new DomainException("Percentage discount cannot exceed 100%");
        }
    }
}
