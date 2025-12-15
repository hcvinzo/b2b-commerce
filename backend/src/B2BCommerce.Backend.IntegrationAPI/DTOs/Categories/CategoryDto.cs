namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Categories;

/// <summary>
/// Category data transfer object for API responses.
/// Id = ExternalId (string), Code = ExternalCode (string).
/// Internal Guid is never exposed.
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// External ID (from source system like LOGO ERP)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// External code (optional secondary reference)
    /// </summary>
    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Parent category's external ID
    /// </summary>
    public string? ParentId { get; set; }
    public string? ParentName { get; set; }

    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string? Slug { get; set; }

    public DateTime? LastSyncedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Category list item for paginated responses.
/// Id = ExternalId (string), Code = ExternalCode (string).
/// Internal Guid is never exposed.
/// </summary>
public class CategoryListDto
{
    /// <summary>
    /// External ID (from source system like LOGO ERP)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// External code (optional secondary reference)
    /// </summary>
    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Parent category's external ID
    /// </summary>
    public string? ParentId { get; set; }
    public string? ParentName { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int SubCategoryCount { get; set; }
    public int ProductCount { get; set; }

    public DateTime? LastSyncedAt { get; set; }
}

/// <summary>
/// Category tree node for hierarchical responses.
/// Id = ExternalId (string), Code = ExternalCode (string).
/// Internal Guid is never exposed.
/// </summary>
public class CategoryTreeDto
{
    /// <summary>
    /// External ID (from source system like LOGO ERP)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// External code (optional secondary reference)
    /// </summary>
    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<CategoryTreeDto> SubCategories { get; set; } = new();

    public DateTime? LastSyncedAt { get; set; }
}
