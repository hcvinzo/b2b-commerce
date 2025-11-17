using B2BCommerce.Backend.Domain.Common;
using B2BCommerce.Backend.Domain.Enums;
using B2BCommerce.Backend.Domain.Exceptions;
using B2BCommerce.Backend.Domain.ValueObjects;

namespace B2BCommerce.Backend.Domain.Entities;

/// <summary>
/// Product entity representing items that can be ordered
/// </summary>
public class Product : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string SKU { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid? BrandId { get; private set; }

    // Pricing
    public Money ListPrice { get; private set; }
    public Money? Tier1Price { get; private set; }
    public Money? Tier2Price { get; private set; }
    public Money? Tier3Price { get; private set; }
    public Money? Tier4Price { get; private set; }
    public Money? Tier5Price { get; private set; }

    // Stock
    public int StockQuantity { get; private set; }
    public int MinimumOrderQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsSerialTracked { get; private set; }

    // Tax
    public decimal TaxRate { get; private set; }

    // Images and metadata
    public string? MainImageUrl { get; private set; }
    public List<string> ImageUrls { get; private set; }
    public Dictionary<string, string> Specifications { get; private set; }

    // Weight and dimensions
    public decimal? Weight { get; private set; }
    public decimal? Length { get; private set; }
    public decimal? Width { get; private set; }
    public decimal? Height { get; private set; }

    // Navigation properties
    public Category? Category { get; set; }
    public Brand? Brand { get; set; }

    private Product() // For EF Core
    {
        Name = string.Empty;
        Description = string.Empty;
        SKU = string.Empty;
        ListPrice = Money.Zero("USD");
        ImageUrls = new List<string>();
        Specifications = new Dictionary<string, string>();
    }

    public Product(
        string name,
        string description,
        string sku,
        Guid categoryId,
        Money listPrice,
        int stockQuantity,
        int minimumOrderQuantity = 1,
        decimal taxRate = 0.18m)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be null or empty", nameof(name));

        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("Product SKU cannot be null or empty", nameof(sku));

        if (stockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(stockQuantity));

        if (minimumOrderQuantity < 1)
            throw new ArgumentException("Minimum order quantity must be at least 1", nameof(minimumOrderQuantity));

        Name = name;
        Description = description ?? string.Empty;
        SKU = sku;
        CategoryId = categoryId;
        ListPrice = listPrice ?? throw new ArgumentNullException(nameof(listPrice));
        StockQuantity = stockQuantity;
        MinimumOrderQuantity = minimumOrderQuantity;
        TaxRate = taxRate;
        IsActive = true;
        IsSerialTracked = false;
        ImageUrls = new List<string>();
        Specifications = new Dictionary<string, string>();
    }

    public void UpdateBasicInfo(string name, string description, Guid categoryId, Guid? brandId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be null or empty", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        CategoryId = categoryId;
        BrandId = brandId;
    }

    public void UpdatePricing(Money listPrice, Money? tier1Price = null, Money? tier2Price = null,
        Money? tier3Price = null, Money? tier4Price = null, Money? tier5Price = null)
    {
        ListPrice = listPrice ?? throw new ArgumentNullException(nameof(listPrice));
        Tier1Price = tier1Price;
        Tier2Price = tier2Price;
        Tier3Price = tier3Price;
        Tier4Price = tier4Price;
        Tier5Price = tier5Price;
    }

    public Money GetPriceForTier(PriceTier tier)
    {
        return tier switch
        {
            PriceTier.List => ListPrice,
            PriceTier.Tier1 => Tier1Price ?? ListPrice,
            PriceTier.Tier2 => Tier2Price ?? ListPrice,
            PriceTier.Tier3 => Tier3Price ?? ListPrice,
            PriceTier.Tier4 => Tier4Price ?? ListPrice,
            PriceTier.Tier5 => Tier5Price ?? ListPrice,
            _ => ListPrice
        };
    }

    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));

        StockQuantity = quantity;
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (StockQuantity < quantity)
            throw new OutOfStockException(Id, quantity, StockQuantity);

        StockQuantity -= quantity;
    }

    public void ReleaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        StockQuantity += quantity;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void SetMainImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be null or empty", nameof(imageUrl));

        MainImageUrl = imageUrl;
    }

    public void AddImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be null or empty", nameof(imageUrl));

        if (!ImageUrls.Contains(imageUrl))
            ImageUrls.Add(imageUrl);
    }

    public void RemoveImage(string imageUrl)
    {
        ImageUrls.Remove(imageUrl);
    }

    public void AddSpecification(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Specification key cannot be null or empty", nameof(key));

        Specifications[key] = value;
    }

    public void UpdateDimensions(decimal? weight, decimal? length, decimal? width, decimal? height)
    {
        Weight = weight;
        Length = length;
        Width = width;
        Height = height;
    }
}
