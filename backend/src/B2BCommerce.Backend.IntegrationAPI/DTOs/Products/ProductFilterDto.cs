namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Products;

/// <summary>
/// Filter parameters for product listing.
/// All entity IDs (CategoryId, BrandId, ProductTypeId) are ExternalIds.
/// </summary>
public class ProductFilterDto
{
    /// <summary>
    /// Search term for name, SKU, or description filtering
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Filter by category external ID
    /// </summary>
    public string? CategoryId { get; set; }

    /// <summary>
    /// Filter by brand external ID
    /// </summary>
    public string? BrandId { get; set; }

    /// <summary>
    /// Filter by product type external ID
    /// </summary>
    public string? ProductTypeId { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Filter by minimum stock quantity
    /// </summary>
    public int? MinStock { get; set; }

    /// <summary>
    /// Filter by maximum stock quantity
    /// </summary>
    public int? MaxStock { get; set; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size (max 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort by field (name, sku, createdAt, updatedAt, stockQuantity, listPrice)
    /// </summary>
    public string SortBy { get; set; } = "name";

    /// <summary>
    /// Sort direction (asc, desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";
}
