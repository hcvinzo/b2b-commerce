namespace B2BCommerce.Backend.Application.DTOs.Categories;

/// <summary>
/// DTO for updating an existing category
/// </summary>
public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
