using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Products;

/// <summary>
/// Product data transfer object for API responses.
/// Id = ExternalId (string), Code = ExternalCode (string).
/// All related entity IDs (CategoryId, BrandId, ProductTypeId) are also ExternalIds.
/// Internal Guids are never exposed.
/// </summary>
public class ProductDto
{
    /// <summary>
    /// External ID (from source system like LOGO ERP)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// External code (optional secondary reference)
    /// </summary>
    public string? Code { get; set; }

    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Category (external ID)
    public string? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    // Brand (external ID)
    public string? BrandId { get; set; }
    public string? BrandName { get; set; }

    // ProductType (external ID)
    public string? ProductTypeId { get; set; }
    public string? ProductTypeName { get; set; }

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
    /// <summary>
    /// Product status (Draft = 0, Active = 1, Inactive = 2)
    /// </summary>
    public ProductStatus Status { get; set; }

    /// <summary>
    /// Whether product is active (computed from Status == Active).
    /// Kept for backward compatibility.
    /// </summary>
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

    public DateTime? LastSyncedAt { get; set; }

    // Variant support
    /// <summary>
    /// Main product's ExternalId (if this is a variant/SKU)
    /// </summary>
    public string? MainProductId { get; set; }

    /// <summary>
    /// Whether this product is a variant (has a MainProductId)
    /// </summary>
    public bool IsVariant { get; set; }

    /// <summary>
    /// Whether this product is a main product (can have variants)
    /// </summary>
    public bool IsMainProduct { get; set; }

    /// <summary>
    /// Number of variants if this is a main product
    /// </summary>
    public int VariantCount { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Product list item for paginated responses.
/// Id = ExternalId (string), Code = ExternalCode (string).
/// Internal Guids are never exposed.
/// </summary>
public class ProductListDto
{
    /// <summary>
    /// External ID (from source system like LOGO ERP)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// External code (optional secondary reference)
    /// </summary>
    public string? Code { get; set; }

    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category's ExternalId
    /// </summary>
    public string? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    /// <summary>
    /// Brand's ExternalId
    /// </summary>
    public string? BrandId { get; set; }
    public string? BrandName { get; set; }

    public decimal ListPrice { get; set; }
    public string Currency { get; set; } = "TRY";
    public int StockQuantity { get; set; }

    /// <summary>
    /// Product status (Draft = 0, Active = 1, Inactive = 2)
    /// </summary>
    public ProductStatus Status { get; set; }

    /// <summary>
    /// Whether product is active (computed from Status == Active).
    /// Kept for backward compatibility.
    /// </summary>
    public bool IsActive { get; set; }
    public string? MainImageUrl { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    // Variant support
    /// <summary>
    /// Main product's ExternalId (if this is a variant/SKU)
    /// </summary>
    public string? MainProductId { get; set; }

    /// <summary>
    /// Whether this product is a variant (has a MainProductId)
    /// </summary>
    public bool IsVariant { get; set; }

    /// <summary>
    /// Number of variants if this is a main product
    /// </summary>
    public int VariantCount { get; set; }
}
