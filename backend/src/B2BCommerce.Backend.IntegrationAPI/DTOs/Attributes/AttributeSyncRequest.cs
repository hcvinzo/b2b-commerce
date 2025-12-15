using System.ComponentModel.DataAnnotations;

namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Attributes;

/// <summary>
/// Request DTO for syncing an attribute definition from external system.
/// Id = ExternalId (string) - the primary upsert key.
/// Code is the business code (like "screen_size"), NOT ExternalCode.
/// </summary>
public class AttributeSyncRequest
{
    /// <summary>
    /// External ID (PRIMARY upsert key).
    /// This is the ID from the source system (LOGO ERP).
    /// Required for creating new attributes.
    /// </summary>
    [StringLength(100)]
    public string? Id { get; set; }

    /// <summary>
    /// Unique business code for the attribute (required, e.g., "screen_size", "memory_capacity").
    /// Used as fallback for matching if Id is not provided.
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
