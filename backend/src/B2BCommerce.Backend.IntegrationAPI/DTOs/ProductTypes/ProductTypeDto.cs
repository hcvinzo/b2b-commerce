namespace B2BCommerce.Backend.IntegrationAPI.DTOs.ProductTypes;

/// <summary>
/// Product type DTO for Integration API
/// </summary>
public class ProductTypeDto
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Unique code for the product type
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description for admin reference
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this product type is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Number of attributes assigned to this product type
    /// </summary>
    public int AttributeCount { get; set; }

    /// <summary>
    /// Attributes assigned to this product type
    /// </summary>
    public List<ProductTypeAttributeDto> Attributes { get; set; } = new();

    /// <summary>
    /// External system ID (primary key for ERP integration)
    /// </summary>
    public string? ExtId { get; set; }

    /// <summary>
    /// External system code (optional reference)
    /// </summary>
    public string? ExtCode { get; set; }

    /// <summary>
    /// When the entity was last synchronized with the external system
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// When the product type was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the product type was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Product type list item DTO for Integration API
/// </summary>
public class ProductTypeListDto
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Unique code for the product type
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this product type is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Number of attributes assigned to this product type
    /// </summary>
    public int AttributeCount { get; set; }

    /// <summary>
    /// External system ID (primary key for ERP integration)
    /// </summary>
    public string? ExtId { get; set; }

    /// <summary>
    /// External system code (optional reference)
    /// </summary>
    public string? ExtCode { get; set; }

    /// <summary>
    /// When the entity was last synchronized with the external system
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }
}

/// <summary>
/// Product type attribute DTO for Integration API
/// </summary>
public class ProductTypeAttributeDto
{
    /// <summary>
    /// Attribute definition ID
    /// </summary>
    public Guid AttributeDefinitionId { get; set; }

    /// <summary>
    /// Attribute code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Attribute display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Attribute display name in English
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// Attribute data type
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Unit of measurement
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Whether this attribute is required for products of this type
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Display order within this product type
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Predefined values for Select/MultiSelect types
    /// </summary>
    public List<AttributeValueOptionDto> PredefinedValues { get; set; } = new();
}

/// <summary>
/// Attribute value option DTO for Select/MultiSelect types
/// </summary>
public class AttributeValueOptionDto
{
    /// <summary>
    /// Value ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Internal value
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Display text for the user
    /// </summary>
    public string? DisplayText { get; set; }

    /// <summary>
    /// Display order in dropdown
    /// </summary>
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Filter DTO for product types
/// </summary>
public class ProductTypeFilterDto
{
    /// <summary>
    /// Search term for name or code
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; } = 20;
}
