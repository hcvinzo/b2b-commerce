namespace B2BCommerce.Backend.Application.DTOs.Categories;

/// <summary>
/// Filter parameters for category listing
/// </summary>
public class CategoryFilterDto
{
    public string? Search { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "displayOrder";
    public string SortDirection { get; set; } = "asc";
}
