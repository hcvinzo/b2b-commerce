namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Attributes;

/// <summary>
/// Full attribute definition response DTO.
/// Id = ExternalId (string), Code here is the business code (not ExternalCode).
/// Internal Guid is never exposed.
/// </summary>
public class AttributeDefinitionDto
{
    /// <summary>
    /// External ID (from source system like LOGO ERP)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Business code (unique identifier like "screen_size", "memory_capacity")
    /// </summary>
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? Unit { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsRequired { get; set; }
    public bool IsVisibleOnProductPage { get; set; }
    public int DisplayOrder { get; set; }

    public DateTime? LastSyncedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<AttributeValueDto>? PredefinedValues { get; set; }
}

/// <summary>
/// Attribute definition list item (without values).
/// Id = ExternalId (string).
/// Internal Guid is never exposed.
/// </summary>
public class AttributeDefinitionListDto
{
    /// <summary>
    /// External ID (from source system like LOGO ERP)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Business code (unique identifier)
    /// </summary>
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? Unit { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsRequired { get; set; }
    public bool IsVisibleOnProductPage { get; set; }
    public int DisplayOrder { get; set; }

    public DateTime? LastSyncedAt { get; set; }
    public int ValueCount { get; set; }
}

/// <summary>
/// Predefined value response DTO.
/// Uses Value as the identifier (unique within attribute).
/// </summary>
public class AttributeValueDto
{
    /// <summary>
    /// The value string (unique within attribute, used as identifier)
    /// </summary>
    public string Value { get; set; } = null!;
    public string? DisplayText { get; set; }
    public int DisplayOrder { get; set; }
}
