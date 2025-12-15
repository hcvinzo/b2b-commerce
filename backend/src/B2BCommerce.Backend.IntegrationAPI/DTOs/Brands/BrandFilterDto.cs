namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Brands;

/// <summary>
/// Filter parameters for brand listing
/// </summary>
public class BrandFilterDto
{
    /// <summary>
    /// Search term for name filtering
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size (max 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort by field (name, createdAt, updatedAt)
    /// </summary>
    public string SortBy { get; set; } = "name";

    /// <summary>
    /// Sort direction (asc, desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";
}
