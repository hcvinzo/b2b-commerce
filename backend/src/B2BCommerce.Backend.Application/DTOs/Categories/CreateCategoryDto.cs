namespace B2BCommerce.Backend.Application.DTOs.Categories;

/// <summary>
/// DTO for creating a new category
/// </summary>
public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
