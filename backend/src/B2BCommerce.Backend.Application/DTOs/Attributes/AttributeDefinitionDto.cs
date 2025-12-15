using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Attributes;

/// <summary>
/// Attribute definition data transfer object for output
/// </summary>
public class AttributeDefinitionDto
{
    /// <summary>
    /// Attribute definition identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Unique code (e.g., "screen_size", "memory_capacity")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name in Turkish
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display name in English
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// Data type for this attribute
    /// </summary>
    public AttributeType Type { get; set; }

    /// <summary>
    /// Unit of measurement (e.g., "GB", "MB/s")
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Whether this attribute should appear in product filters
    /// </summary>
    public bool IsFilterable { get; set; }

    /// <summary>
    /// Default required status
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Whether to display on product detail page
    /// </summary>
    public bool IsVisibleOnProductPage { get; set; }

    /// <summary>
    /// Display order in UI
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Predefined values for Select/MultiSelect types
    /// </summary>
    public List<AttributeValueDto> PredefinedValues { get; set; } = new();

    /// <summary>
    /// External system ID (primary upsert key for ERP sync)
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// External system code (optional reference)
    /// </summary>
    public string? ExternalCode { get; set; }

    /// <summary>
    /// When the entity was last synced with external system
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// Date created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
