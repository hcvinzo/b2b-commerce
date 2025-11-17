namespace B2BCommerce.Backend.Application.DTOs.Products;

/// <summary>
/// Data transfer object for updating an existing product
/// </summary>
public class UpdateProductDto
{
    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category identifier
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Brand identifier (optional)
    /// </summary>
    public Guid? BrandId { get; set; }

    /// <summary>
    /// List price amount
    /// </summary>
    public decimal ListPrice { get; set; }

    /// <summary>
    /// Currency code (e.g., USD, EUR, TRY)
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Tier 1 price amount (optional)
    /// </summary>
    public decimal? Tier1Price { get; set; }

    /// <summary>
    /// Tier 2 price amount (optional)
    /// </summary>
    public decimal? Tier2Price { get; set; }

    /// <summary>
    /// Tier 3 price amount (optional)
    /// </summary>
    public decimal? Tier3Price { get; set; }

    /// <summary>
    /// Tier 4 price amount (optional)
    /// </summary>
    public decimal? Tier4Price { get; set; }

    /// <summary>
    /// Tier 5 price amount (optional)
    /// </summary>
    public decimal? Tier5Price { get; set; }

    /// <summary>
    /// Stock quantity
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Minimum order quantity
    /// </summary>
    public int MinimumOrderQuantity { get; set; }

    /// <summary>
    /// Whether product is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Whether product is serial tracked
    /// </summary>
    public bool IsSerialTracked { get; set; }

    /// <summary>
    /// Tax rate (e.g., 0.18 for 18%)
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Main image URL
    /// </summary>
    public string? MainImageUrl { get; set; }

    /// <summary>
    /// Additional image URLs
    /// </summary>
    public List<string>? ImageUrls { get; set; }

    /// <summary>
    /// Product specifications
    /// </summary>
    public Dictionary<string, string>? Specifications { get; set; }

    /// <summary>
    /// Weight in kilograms
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Length in centimeters
    /// </summary>
    public decimal? Length { get; set; }

    /// <summary>
    /// Width in centimeters
    /// </summary>
    public decimal? Width { get; set; }

    /// <summary>
    /// Height in centimeters
    /// </summary>
    public decimal? Height { get; set; }
}
