using System.ComponentModel.DataAnnotations;

namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Attributes;

/// <summary>
/// Request DTO for syncing an attribute definition from external system.
/// Uses ExtId (primary) or Id (internal) as the upsert key.
/// One of ExtId or Id is required.
/// </summary>
public class AttributeSyncRequest
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
    /// Unique code for the attribute (required, e.g., "screen_size", "memory_capacity")
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// Display name in Turkish (required)
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Data type for this attribute (required).
    /// Valid values: Text, Number, Select, MultiSelect, Boolean, Date
    /// </summary>
    [Required]
    public string Type { get; set; } = null!;

    /// <summary>
    /// Unit of measurement (optional, e.g., "GB", "MB/s", "mm")
    /// </summary>
    [StringLength(50)]
    public string? Unit { get; set; }

    /// <summary>
    /// Whether this attribute should appear in product filters
    /// </summary>
    public bool IsFilterable { get; set; } = true;

    /// <summary>
    /// Default required status (can be overridden per ProductType)
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Whether to display on product detail page
    /// </summary>
    public bool IsVisibleOnProductPage { get; set; } = true;

    /// <summary>
    /// Display order in UI
    /// </summary>
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Predefined values for Select/MultiSelect types (full replacement on update)
    /// </summary>
    [MaxLength(500)]
    public List<AttributeValueSyncRequest>? Values { get; set; }
}

/// <summary>
/// Request DTO for syncing a predefined value
/// </summary>
public class AttributeValueSyncRequest
{
    /// <summary>
    /// The value (required, used as key for matching existing values)
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Value { get; set; } = null!;

    /// <summary>
    /// User-facing display text (optional, falls back to Value if not provided)
    /// </summary>
    [StringLength(500)]
    public string? DisplayText { get; set; }

    /// <summary>
    /// Display order in dropdowns/lists
    /// </summary>
    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; } = 0;
}

/// <summary>
/// Request DTO for bulk syncing attribute definitions
/// </summary>
public class BulkAttributeSyncRequest
{
    /// <summary>
    /// List of attributes to sync
    /// </summary>
    [Required]
    [MinLength(1)]
    [MaxLength(500)]
    public List<AttributeSyncRequest> Attributes { get; set; } = new();
}
