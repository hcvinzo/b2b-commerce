namespace B2BCommerce.Backend.IntegrationAPI.DTOs.ProductTypes;

/// <summary>
/// Product type DTO for Integration API.
/// Id = ExternalId (string).
/// Code is the business code (not ExternalCode).
/// Internal Guid is never exposed.
/// </summary>
public class ProductTypeDto
{
    /// <summary>
    /// External ID (from source system like LOGO ERP)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Unique business code for the product type
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
/// Product type list item DTO for Integration API.
/// Id = ExternalId (string).
/// Internal Guid is never exposed.
/// </summary>
public class ProductTypeListDto
{
    /// <summary>
    /// External ID (from source system like LOGO ERP)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Unique business code for the product type
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
    /// When the entity was last synchronized with the external system
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }
}

/// <summary>
/// Product type attribute DTO for Integration API.
/// AttributeId = Attribute's ExternalId (string).
/// </summary>
public class ProductTypeAttributeDto
{
    /// <summary>
    /// Attribute's external ID
    /// </summary>
    public string AttributeId { get; set; } = string.Empty;

    /// <summary>
    /// Attribute business code
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
/// Attribute value option DTO for Select/MultiSelect types.
/// Uses Value as the identifier (unique within attribute).
/// </summary>
public class AttributeValueOptionDto
{
    /// <summary>
    /// Internal value (used as identifier, unique within attribute)
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
