using System.ComponentModel.DataAnnotations;

namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Categories;

/// <summary>
/// Request DTO for syncing a category from external system.
/// Id = ExternalId (string) - the primary upsert key.
/// Code = ExternalCode (string) - optional secondary reference.
/// </summary>
public class CategorySyncRequest
{
    /// <summary>
    /// External ID (PRIMARY upsert key).
    /// This is the ID from the source system (LOGO ERP).
    /// Required for creating new categories.
    /// </summary>
    [StringLength(100)]
    public string? Id { get; set; }

    /// <summary>
    /// External code (OPTIONAL secondary reference)
    /// </summary>
    [StringLength(100)]
    public string? Code { get; set; }

    /// <summary>
    /// Category name
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Category description
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Parent category's external ID (for hierarchy)
    /// </summary>
    [StringLength(100)]
    public string? ParentId { get; set; }

    /// <summary>
    /// Image URL
    /// </summary>
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Display order
    /// </summary>
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Active status
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request DTO for bulk syncing categories from external system.
/// </summary>
public class BulkCategorySyncRequest
{
    /// <summary>
    /// List of categories to sync
    /// </summary>
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public List<CategorySyncRequest> Categories { get; set; } = new();
}
