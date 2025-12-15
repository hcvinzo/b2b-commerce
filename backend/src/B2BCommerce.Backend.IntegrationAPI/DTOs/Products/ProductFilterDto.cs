namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Products;

/// <summary>
/// Filter parameters for product listing
/// </summary>
public class ProductFilterDto
{
    /// <summary>
    /// Search term for name, SKU, or description filtering
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Filter by category ID
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Filter by category external ID (alternative to CategoryId)
    /// </summary>
    public string? CategoryExtId { get; set; }

    /// <summary>
    /// Filter by brand ID
    /// </summary>
    public Guid? BrandId { get; set; }

    /// <summary>
    /// Filter by product type ID
    /// </summary>
    public Guid? ProductTypeId { get; set; }

    /// <summary>
    /// Filter by product type external ID (alternative to ProductTypeId)
    /// </summary>
    public string? ProductTypeExtId { get; set; }

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
