using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Products;

/// <summary>
/// Simplified product data transfer object for list views
/// </summary>
public class ProductListDto
{
    /// <summary>
    /// Product identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Stock Keeping Unit
    /// </summary>
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Category name
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Brand name
    /// </summary>
    public string? BrandName { get; set; }

    /// <summary>
    /// List price amount
    /// </summary>
    public decimal ListPrice { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Stock quantity
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Product status (Draft, Active, Inactive)
    /// </summary>
    public ProductStatus Status { get; set; }

    /// <summary>
    /// Whether product is active (computed from Status == Active)
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Main image URL
    /// </summary>
    public string? MainImageUrl { get; set; }
}
