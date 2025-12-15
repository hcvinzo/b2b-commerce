namespace B2BCommerce.Backend.IntegrationAPI.DTOs.Attributes;

/// <summary>
/// Full attribute definition response DTO
/// </summary>
public class AttributeDefinitionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? Unit { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsRequired { get; set; }
    public bool IsVisibleOnProductPage { get; set; }
    public int DisplayOrder { get; set; }
    public string? ExternalId { get; set; }
    public string? ExternalCode { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<AttributeValueDto>? PredefinedValues { get; set; }
}

/// <summary>
/// Attribute definition list item (without values)
/// </summary>
public class AttributeDefinitionListDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? Unit { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsRequired { get; set; }
    public bool IsVisibleOnProductPage { get; set; }
    public int DisplayOrder { get; set; }
    public string? ExternalId { get; set; }
    public string? ExternalCode { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public int ValueCount { get; set; }
}

/// <summary>
/// Predefined value response DTO
/// </summary>
public class AttributeValueDto
{
    public Guid Id { get; set; }
    public string Value { get; set; } = null!;
    public string? DisplayText { get; set; }
    public int DisplayOrder { get; set; }
}
