using System.ComponentModel.DataAnnotations;

namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Categories;

/// <summary>
/// DTO for creating a new category
/// </summary>
public class CreateCategoryDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}
