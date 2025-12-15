namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Categories;

/// <summary>
/// Filter parameters for category listing
/// </summary>
public class CategoryFilterDto
{
    /// <summary>
    /// Search term for name filtering
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Filter by parent category ID (null for root categories)
    /// </summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>
    /// Filter by parent category's external ID (alternative to ParentCategoryId)
    /// </summary>
    public string? ParentCategoryExtId { get; set; }

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
    /// Sort by field (name, displayOrder, createdAt)
    /// </summary>
    public string SortBy { get; set; } = "displayOrder";

    /// <summary>
    /// Sort direction (asc, desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";
}
