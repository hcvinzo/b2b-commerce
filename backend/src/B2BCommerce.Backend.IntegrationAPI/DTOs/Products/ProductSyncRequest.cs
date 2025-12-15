using System.ComponentModel.DataAnnotations;

namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Products;

/// <summary>
/// Request DTO for syncing a product from external system (LOGO ERP).
/// Uses ExtId (primary), Id (internal), or SKU as the upsert key.
/// ExtId is required for creating new products.
/// </summary>
public class ProductSyncRequest
{
    /// <summary>
    /// Internal ID (optional - for internal updates)
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// External system ID (PRIMARY upsert key - required for new products)
    /// </summary>
    [StringLength(100)]
    public string? ExtId { get; set; }

    /// <summary>
    /// External system code (OPTIONAL reference)
    /// </summary>
    [StringLength(100)]
    public string? ExtCode { get; set; }

    /// <summary>
    /// Stock Keeping Unit (required, unique, used as fallback for matching)
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string SKU { get; set; } = null!;

    /// <summary>
    /// Product name
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Product description
    /// </summary>
    [StringLength(5000)]
    public string? Description { get; set; }

    // Category lookup (one is required)

    /// <summary>
    /// Category internal ID
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Category external ID (alternative to CategoryId)
    /// </summary>
    [StringLength(100)]
    public string? CategoryExtId { get; set; }

    // Brand lookup (optional)

    /// <summary>
    /// Brand internal ID
    /// </summary>
    public Guid? BrandId { get; set; }

    // ProductType lookup (optional)

    /// <summary>
    /// Product type internal ID
    /// </summary>
    public Guid? ProductTypeId { get; set; }

    /// <summary>
    /// Product type external ID (alternative to ProductTypeId)
    /// </summary>
    [StringLength(100)]
    public string? ProductTypeExtId { get; set; }

    // Pricing

    /// <summary>
    /// List price amount (required)
    /// </summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal ListPrice { get; set; }

    /// <summary>
    /// Currency code (default: TRY)
    /// </summary>
    [StringLength(3)]
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Tier 1 price amount
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? Tier1Price { get; set; }

    /// <summary>
    /// Tier 2 price amount
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? Tier2Price { get; set; }

    /// <summary>
    /// Tier 3 price amount
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? Tier3Price { get; set; }

    /// <summary>
    /// Tier 4 price amount
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? Tier4Price { get; set; }

    /// <summary>
    /// Tier 5 price amount
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? Tier5Price { get; set; }

    // Stock

    /// <summary>
    /// Current stock quantity
    /// </summary>
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; } = 0;

    /// <summary>
    /// Minimum order quantity
    /// </summary>
    [Range(1, int.MaxValue)]
    public int MinimumOrderQuantity { get; set; } = 1;

    // Tax

    /// <summary>
    /// Tax rate (e.g., 0.20 for 20%)
    /// </summary>
    [Range(0, 1)]
    public decimal TaxRate { get; set; } = 0.20m;

    // Status

    /// <summary>
    /// Whether product is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Images

    /// <summary>
    /// Main product image URL
    /// </summary>
    [StringLength(500)]
    public string? MainImageUrl { get; set; }

    /// <summary>
    /// Additional image URLs (max 20)
    /// </summary>
    [MaxLength(20)]
    public List<string>? ImageUrls { get; set; }

    // Dimensions

    /// <summary>
    /// Weight in kilograms
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? Weight { get; set; }

    /// <summary>
    /// Length in centimeters
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? Length { get; set; }

    /// <summary>
    /// Width in centimeters
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? Width { get; set; }

    /// <summary>
    /// Height in centimeters
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? Height { get; set; }
}

/// <summary>
/// Request DTO for bulk syncing products from external system.
/// </summary>
public class BulkProductSyncRequest
{
    /// <summary>
    /// List of products to sync
    /// </summary>
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public List<ProductSyncRequest> Products { get; set; } = new();
}
