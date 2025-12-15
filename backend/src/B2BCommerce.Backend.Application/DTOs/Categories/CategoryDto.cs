namespace B2BCommerce.Backend.Application.DTOs.Categories;

/// <summary>
/// Category data transfer object
/// </summary>
public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public string? ParentExternalId { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string? Slug { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // External entity fields
    public string? ExternalCode { get; set; }
    public string? ExternalId { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}

/// <summary>
/// Category list item for paginated responses
/// </summary>
public class CategoryListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public string? ParentExternalId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int SubCategoryCount { get; set; }
    public int ProductCount { get; set; }

    // External entity fields
    public string? ExternalCode { get; set; }
    public string? ExternalId { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}

/// <summary>
/// Category tree node for hierarchical responses
/// </summary>
public class CategoryTreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<CategoryTreeDto> SubCategories { get; set; } = new();

    // External entity fields
    public string? ExternalCode { get; set; }
    public string? ExternalId { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}
