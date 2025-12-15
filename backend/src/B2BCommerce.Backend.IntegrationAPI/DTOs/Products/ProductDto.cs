namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Products;

/// <summary>
/// Product data transfer object for API responses
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Category
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryExtId { get; set; }

    // Brand
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }

    // ProductType
    public Guid? ProductTypeId { get; set; }
    public string? ProductTypeName { get; set; }
    public string? ProductTypeExtId { get; set; }

    // Pricing
    public decimal ListPrice { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal? Tier1Price { get; set; }
    public decimal? Tier2Price { get; set; }
    public decimal? Tier3Price { get; set; }
    public decimal? Tier4Price { get; set; }
    public decimal? Tier5Price { get; set; }

    // Stock
    public int StockQuantity { get; set; }
    public int MinimumOrderQuantity { get; set; }

    // Tax
    public decimal TaxRate { get; set; }

    // Status
    public bool IsActive { get; set; }
    public bool IsSerialTracked { get; set; }

    // Images
    public string? MainImageUrl { get; set; }
    public List<string> ImageUrls { get; set; } = new();

    // Dimensions
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }

    // External entity fields
    public string? ExtId { get; set; }
    public string? ExtCode { get; set; }
    public DateTime? LastSyncedAt { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Product list item for paginated responses
/// </summary>
public class ProductListDto
{
    public Guid Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? BrandName { get; set; }
    public decimal ListPrice { get; set; }
    public string Currency { get; set; } = "TRY";
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public string? MainImageUrl { get; set; }

    // External entity fields
    public string? ExtId { get; set; }
    public string? ExtCode { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}
