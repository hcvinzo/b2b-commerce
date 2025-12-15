using B2BCommerce.Backend.Domain.Enums;

namespace B2BCommerce.Backend.Application.DTOs.Attributes;

/// <summary>
/// DTO for creating a new attribute definition
/// </summary>
public class CreateAttributeDefinitionDto
{
    /// <summary>
    /// Unique code (e.g., "screen_size", "memory_capacity")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name in Turkish
    /// </summary>
    public string Name { get; set; } = string.Empty;

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
    public bool IsFilterable { get; set; } = true;

    /// <summary>
    /// Default required status
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Whether to display on product detail page
    /// </summary>
    public bool IsVisibleOnProductPage { get; set; } = true;

    /// <summary>
    /// Display order in UI
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Initial predefined values for Select/MultiSelect types
    /// </summary>
    public List<CreateAttributeValueDto>? PredefinedValues { get; set; }
}

/// <summary>
/// DTO for creating a predefined attribute value
/// </summary>
public class CreateAttributeValueDto
{
    /// <summary>
    /// Internal value (e.g., "256", "red")
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// User-facing display text (e.g., "256 GB", "Red")
    /// </summary>
    public string? DisplayText { get; set; }

    /// <summary>
    /// Display order in dropdowns/lists
    /// </summary>
    public int DisplayOrder { get; set; }
}
