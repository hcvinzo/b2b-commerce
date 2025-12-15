using System.ComponentModel.DataAnnotations;

namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Categories;

/// <summary>
/// Request DTO for syncing a category from external system.
/// Uses ExtId (primary) or Id (internal) as the upsert key.
/// One of ExtId or Id is required.
/// </summary>
public class CategorySyncRequest
{
    /// <summary>
    /// Internal ID (optional for upsert - if provided without ExtId, ExtId will be set to Id.ToString())
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// External system ID (PRIMARY upsert key)
    /// One of ExtId or Id is required.
    /// </summary>
    [StringLength(100)]
    public string? ExtId { get; set; }

    /// <summary>
    /// External system code (OPTIONAL reference)
    /// </summary>
    [StringLength(100)]
    public string? ExtCode { get; set; }

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
    /// Parent category's external ID (for hierarchy - PRIMARY)
    /// </summary>
    [StringLength(100)]
    public string? ParentId { get; set; }

    /// <summary>
    /// Parent category's external code (for hierarchy - FALLBACK)
    /// </summary>
    [StringLength(100)]
    public string? ParentCode { get; set; }

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
