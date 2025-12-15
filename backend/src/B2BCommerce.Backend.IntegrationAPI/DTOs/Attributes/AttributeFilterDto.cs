namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Attributes;

/// <summary>
/// Filter parameters for attribute definition listing
/// </summary>
public class AttributeFilterDto
{
    /// <summary>
    /// Search term for name/code filtering
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Filter by filterable status
    /// </summary>
    public bool? IsFilterable { get; set; }

    /// <summary>
    /// Filter by attribute type (Text, Number, Select, MultiSelect, Boolean, Date)
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Include predefined values in response
    /// </summary>
    public bool IncludeValues { get; set; } = false;

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size (max 100)
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Sort by field (code, name, displayOrder, createdAt)
    /// </summary>
    public string SortBy { get; set; } = "displayOrder";

    /// <summary>
    /// Sort direction (asc, desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";
}
